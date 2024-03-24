CloudTrailIngestor
https://github.com/ehudkleinsde/CloudTrailIngestor/tree/redis

# System Components:
1. CloudTrailIngestor - REST API, sends CloudTrail events to a Kafka topic after schema validation and (simple) dedup.

2. Anomaly Detection backgroud service - has multiple instances of 2 kinds of worker threads, simulating 2 different anomalies detection types.
It runs multiple worker threads for AnomalyType1 and AnomalyType2. These threads read events from Kafka and process them.
They writes results to db when score > 0. They can perform dedup based on data in the db but currently this is commented out and dedup is only done on API side.

3. CloudTrailProvider - For stress testing the system. Randomly generates and pushes CloudTrail events to the ingestor api at a high rate.

4. Dedup - Done based on the combination of EventType and EventID, only for events arriving less than 1hr apart.
	a. CloudTrailIngestor has an in-mem cache with 1hr TTL. Consumes about 1GB of RAM for every 1M unique messages.
	b. Anomaly Detection Workers - can check if an event already has a processing result in the db, and avoid recalculating.
	DB is indexed appropriatley to support this. (currently commented out). This doesnt help in case the event's anomaly score is 0.

# Stress Test results:
On an i7-13700K cpu desktop machine, 128GB DDR4 3600GHz, PCIe4 SSD nvme 7GB/s 1M IOPS, 
Kafka set up with a topic, 40 partitions, 2 consumer groups:

Single anomaly type, 20 detection worker threads:
1. Pushing 1M randomly generated CloudTrail events through a single REST api app, via 2 concurrent pusher threads, into Kafka, took 95sec, 10.5k events per second.
2. Total processing time, until finishing generating 1M records on MongoDB, took 108sec, 9.25k events per second.

Two anomaly types, 20 worker threads each (40 total):
1. Pushing all CloudTrail events - same as above
2. Total processing time, generating 2M records on MongoDB, took 204sec, ~5k events per second (linear).

All events were written to the DB on all runs.

# Setup instructions:
1. Install MongoDB Compass UI tool - https://www.mongodb.com/try/download/compass, so you can watch anomalies written to the db at real time.
2. In the command line, navigate to CloudTrailIngestor. Run "docker-compose -f docker-compose.yml up -d". 
This will start everything. Images are shared on DockerHub. This uses Kafka, ZooKeeper and MongoDB default images.
3. Connect to the docker mongodb instace in MongoDB Compass UI tool via this connection string - "mongodb://localhost:27018/"
4. On your local machine, open the CloudTrailIngestor API swagger UI - http://localhost:5255/swagger/index.html, so you can manually test.
5. Install Redis GUI tool, to see the dedup set - https://redis.com/redis-enterprise/redis-insight/
# Troubleshooting:
1. If rate is low - When the containers load, run to verify kafka has 40 partitions (get containerID via docker ps):
docker exec -it <containerID> kafka-topics.sh --bootstrap-server kafka:9092 --describe --topic cloudtrailtopic

STRESS Test Flow:
0. Run compose.
1. Wait for 1min, as the consumer and producer apps are dormant for the first 1min until kafka, ZooKeeper and mongo containers are up and running.
2. On Docker, go to Containers, go to the cloudtrailprovider logs, wait for it to start generating events.
3. Go to anomalydetection, watch it process events and persist them to the db.
4. Go to MongoDB Compass, refresh the db list, see a new db was created.
5. Check the content of the new db, click refresh several times, watch events being written at a high rate.
6. Wait for the amount of items in the db to reach exactly 2M.

Simple manual test:
(start by deleting the MongoDB db, so we get a fresh start)

Sanity: Post a new CloudTrail event:
0. Delete MongoDB AnomalyDetectionResult db if exists. In Docker, clear anomalydetection logs
1. Go to http://localhost:5255/swagger/index.html, the cloudTrailIngstor API swagger. Post a new message. The default swagger values are ok.
2. See the log in anomalydetection logs on Docker. See the new event in the db.

Dedup test:
1. Post the same cloudTrail event again.
2. See no new events in the db and no logs in anomalydetection logs on docker.

#Automatic tests:
1. Unfortunatley no time to write Unit and E2E tests, but these would be very easy via mocking the components.

# Processing result example:

{
  "_id": {
    "$oid": "65fcaa8498a717c0354ecdf3"
  },
  "CloudTrail": {
    "RequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "EventId": "3fa85f64-5727-4562-b3fc-2c963f66afa6",
    "RoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "TimestampUTC": {
      "$date": "2024-03-21T21:39:28.851Z"
    },
    "AffectedAssets": [
      "string"
    ],
    "EventType": "Create"
  },
  "AnomalyScore": 1705159422,
  "AnomalyDetectionType": "Anomaly2",
  "AnomalyDetectionVersion": "1",
  "ProcessingTimestampUTC": {
    "$date": "2024-03-21T21:45:40.061Z"
  }
}

Hardware Requirements:
1. The CloudTrailIngestor service will have around 1GB of RAM available for every 1M ingested events within an hour, for dedup cache.
The throughput acheived on my machine was 10k events per second, so 36M events per hour, so need around 36GB of cache for a whole hour, just for cache, 
so the machine should have that plus additional RAM for OS and its other operations.

# Assumptions I made:
1. If a duplicate event arrived 1Hr after the original event, its not a duplicate. 
2. Dedup is done to save time and resources, but duplications in the db are not very severe, so dedup can be done on best effort and not a hard requirement.
I have a commented out line in the code to check for dedups in the db if this isn't a valid assumption. It will be slower to dedup based on db, but the code creates the appropriate indexes.
If the dedup is a strict requirement, the DB becomes read-heavy, and should be repliacted into read replicas to scale up reads.
See: AlreadyExists function on AnomalyDetectionWorkerBase.cs.
3. Events are uinquiley identified based on EventType and EventID concatanation.

# Further performance tuning suggestions:
Bottleneck seems to be in reading from Kafka and writing to the DB. DB is write heavy.

Single machine optimizations:
1. Considering writing to the DB in batches, instead of one event at a time, which is done currently. 
This should include increasing the time until the consumer performs auto-commit to Kafka, such that if the write fails, the messages aren't removed.
2. Optimize Kafka and mongoDB params, currently everything is set to default values.
In MongoDB, consider journaling interval, compression. In Kafka consider lingering.
3. Serialize remove property names when serializing cloudTrail events, to make the documents in the db lighter, e.g: in the db record, change
"RequestId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", to "rid": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
4. If no need to dedup based on data in the db, can give up the index for faster writes.


Multiple machines design:

Load partitioning:
1. Consider splitting the pipeline by EventType, Read events can be processed in one pipeline, the other types in another pipeline.
Have dedicated Kafka and MongoDB instances for each.
Rest API controller can decide where to write the event to based on the event type.
Pros: Simple partition scheme. Cons: Increases infra complexity. No need for hashing based schema.
Note: Create and Modify events are more interesting, security-wise and seperating them to a faster pipeline might serve business purposes better.

Each pipeline will be agnostic to the events it writes, so both pipelines can handle all types of event types.
Therefore, this will contribute to reseliency, as if one pipeline fails, can redirect traffic to the second one.

# Engineering:
1. Modularity/Testability - all major components are in dedicated, seperate libs, consumed using DI, easily mockable.
2. Anomaly detection workers - Very easy to add a new type of anomaly detection, just implement the abstract AnomalyDetectionWorkerBase class.
Having the Kafka ConsumerConfig in the child class was my mistake, should be in the base class as well :-)
3. Very easy to add more worker threads, can control how many worker threads of each type [there is probably a nicer way to handle the DI registration, sorry.. :-) ]

# Alternative Dedup design suggestions:
1. Implement a bloom filter - for each event identifier, run N hash functions, each returning an integer.
Mod the integers and use them as indexes in a bit integer. If all N respective bits are set to 1, there is a probability that
the event is duplicated - need to check the db. If even 1 bit is set to 0, the event is not duplicated.
Lastly, set all the 0 bits encountered to 1.
Not going into probabilistic details here.. :-)

Dedup alternatives if assumption is invalid:
1. As said in the beginning, I assumed dedup is good to have, but not a high priority:
a. We don't get too many duplicated events from AWS
b. When we do, they arrive within the same hour, and our API controller is stable and does not reset often so cache is solid
c. Even if we get some in, its not a big deal, just consumes compute and storage resources.



# Missing features:
1. We currently don't have rate limiting and auth - add an API gateway to provide these, and redirect valid calls to the downstream.


# Error handling:
We currently do not have anything to handle dropped CloudTrail events besides logging.