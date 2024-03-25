CloudTrailIngestor
https://github.com/ehudkleinsde/CloudTrailIngestor/tree/redis

# System Components:
1. CloudTrailIngestor - REST API controller, gets CloudTrail events and adds them to a Kafka topic after schema validation and dedup 
against a local in-mem cache with 1hr TTL, and Redis cache.

2. Anomaly Detection backgroud service - runs 2 kinds of anomaly detectors, simulating 2 different anomalies detection mechanisms.
Each anomaly detector runs multiple threads (hard coded 20 for demonstration purposes).

3. CloudTrailProvider - For benchmark/stress test. Randomly generates and pushes CloudTrail events to the ingestor api at a high rate.
(set to push 200K events via 2 concurrent worker threads. 
Its possible to run the CloudTrailProvider numerous times to ingest more events, just restart the CloudTrailProvider container after it shuts down.
You can delete the keys in Redis and the records in MongoDB, in between runs, or keep them).

# Throughput acheived - stress test results:
On an i7-13700K cpu desktop machine, 128GB DDR4 3600GHz, PCIe4 SSD nvme 7GB/s 1M IOPS, liquid cooling,
Kafka set up with a topic, 40 partitions, 2 consumer groups, and Redis all default settings, both single nodes,
MongoDB has a composite index on EventType and EventId
Pushing events to the api for all scenarios is done via 50 concurrent threads, each pushing 4k events.
Time to POST all the events, which includes schema validation and writing them to Kafka, is ~13sec (obviously the consumers kick in before this ends).

Scenarios:
1. Single Anomaly type: 15 seconds end to end, for a total avg throughput of 200k/15=~13k events/s, 47M events per hour.
2. 2 anomaly types (so we write 400k events to MongoDB) - 30 seconds end to end (scales linearly), for a total avg throughput of 200k/30=~7k events/s, 25M events per hour.

# Setup instructions:
GUI tools:
1. Install MongoDB Compass UI tool - https://www.mongodb.com/try/download/compass, so you can watch anomalies written to the db at real time. Connect on mongodb://localhost:27018/.
2. Install Redis Insight GUI tool, to see event ids added to the dedup set in real time - https://redis.com/redis-enterprise/redis-insight/. Connect on 127.0.0.1:6379.

Docker images:
1. Using Docker compose file "docker-compose.redis.yml" attached to the email, build the environment: "docker-compose -f docker-compose.redis.yml up -d"

API:
1. On your local machine, open the CloudTrailIngestor API swagger UI - http://localhost:5255/swagger/index.html

How to stress test:
On Docker:
0. Run compose. See the containers are up.
1. Go to CloudTrailProvider container logs, see the program says its waiting, then says its generating events.
2. When CloudTrailProvider says its generating events, go to MongoDB Compass, refresh the db list, see a new db was created.
3. Check the content of the new db, click refresh several times, watch events being written at a high rate.
4. Go to Redis Insight, refresh, see the set of event id keys.
6. Wait for the amount of items in MongoDB to reach 200k.
7. Copy the time when the CloudTrailProvider started generating events from it's logs.
8. Copy the time when the anomalydetection processed its last event.
9. Calculate the different to the end to end processing time.

Simple manual test:
1. Delete the MongoDB db, and the Redis set, clear anomalydetection service logs on docker.

Sanity test: Post a new CloudTrail event:
1. Go to http://localhost:5255/swagger/index.html, the cloudTrailIngstor API swagger. Post a new message. The default swagger values are ok.
2. See the log in anomalydetection logs on Docker. See the new event in the db. See the event's ket in Redis.

In-mem dedup test:
1. Post the same cloudTrail event again.
2. See no new events in the db and no logs in anomalydetection logs on docker.
3. See the event id in the Redis key set.

Redis based dedup:
0. Restart the cloudTrailIngstor container, such that the api's in-mem cache gets deleted.
1. Post the same cloudTrail event again.
2. See no new events in the db and no logs in anomalydetection logs on docker.

# Dedup mechanism - explanation:
Done based on the combination of EventType and EventID.
	a. In-mem: CloudTrailIngestor has an in-mem cache with 1hr TTL. Consumes about 1GB of RAM for every 1M unique messages. So the machine needs 15GB of RAM for 1hr TTL.
	b. If key not found on in-mem cache, check Redis key set.

#Automatic unit/e2e tests:
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
1. The CloudTrailIngestor service will have around 1GB of RAM available for every 1M ingested events, for dedup cache.
The throughput acheived on my machine was 5k events per second, so 18M events per hour, so need around 18GB of cache for a whole hour, just for cache.
2. On Redis Insight, you can see that the set is 17MB, for 200K events. Let's round to 100MB for 1M events. So for each 1B events, we need 100GB of RAM for Redis (can be in multiple node machines).

# Assumptions I made:
Events are uinquiley identified based on EventType and EventID concatanation.

Priliminary multiple machines design suggestion:

Load partitioning:
1. Splitting pipeline by EventType, Read events can be processed in one pipeline, all the other types in another pipeline.
Each pipeline will have a dedicated Kafka, Redis and MongoDB instances (not an even distribution but can have different setup sizes).
2. Rest API controller can direct requests based on the event type.
Pros: Simple partition scheme. No need for hashing based schema. Cons: Increases infra complexity. Not distributing load evenly.
Note: Create and Modify CloudTrail events are more interesting, security-wise, and seperating them to a faster pipeline might serve business purposes better.

Each pipeline will be agnostic to the events it writes, so both pipelines can handle all types of event types.
Therefore, this will contribute to reseliency, as if one pipeline fails, can redirect traffic to the second one.

# Engineering:
1. Modularity/Testability - all major components are in dedicated, seperate libs, consumed using DI, easily mockable.
2. Anomaly detection workers - Very easy to add a new type of anomaly detection, just implement the abstract AnomalyDetectionWorkerBase class.
Having the Kafka ConsumerConfig in the child class was my mistake, should be in the base class as well :-)
3. Very easy to add more worker threads, can control how many worker threads of each type [there is probably a nicer way to handle the DI registration, sorry.. :-) ]

# Missing features:
1. We currently don't have rate limiting and auth - add an API gateway to provide these, and redirect valid calls to the downstream.


# Error handling:
We currently do not have anything to handle dropped CloudTrail events besides logging.