CloudTrailIngestor

Setup:
1. In the command line, navigate to CloudTrailIngestor\Docker. Run "docker-compose up". This will start everything. Images are shared on DockerHub.
2. On your local machine, open the CloudTrailIngestor API swagger UI - http://localhost:5255/swagger/index.html.
4. Install MongoDB Compass UI tool - https://www.mongodb.com/try/download/compass
5. Connect to the docker mongodb instace in Compass via this connection string - "mongodb://localhost:27018/"

Intro:
1. CloudTrailIngestor REST API - to post CloudTrail events. Just a passthrough to the in-memory message broker. Has cache to dedup cloudTrailEvents.
2. Anomalydetection backgroud service - performs polling on the message broker every 5sec, pulls cloudTrail events, and process them. Writes results to db when needed, after making sure they are not in the db already.

Dedup:
1. CloudTrailIngestor has a co-located cache with 1hr TTL.
2. Anomaly Detection Workers - check if the event already has a processing result in the db, if so the event isn't processed.

Processing:
1. The system has 3 types on anomaly processors, AnomalyDetectionWorker1, AnomalyDetectionWorker2, and AnomalyDetectionWorker3, representing different detectors of different anomaly types.
2. Each one has a dedicated queue.
3. Each posted cloudTrail event is duplicated to the 3 queues, to simulate a topic.
4. The workers are running concurrently, polling their respective queues, and process the events.
5. Results are stored in the db in the following format, representing the anomaly score, the anomaly detector and its version.


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
  "AnomalyDetectionType": "Anomaly3",
  "AnomalyDetectionVersion": "1",
  "ProcessingTimestampUTC": {
    "$date": "2024-03-21T21:45:40.061Z"
  }
}


Scale and Resiliency:
1. CloudTrailIngestor REST API is just a passthrough to the message broker, only performing input validation and dedup using local cache.
Therefore, provided we can place a load balancer infront of the API, we can have several instances of the API, which will improve sacle and resiliency.
We can split the load according to the content of the cloudTrail, for example group event types. Read events can go to one instance, Create, Delete and Update, can go to another instance.


2. 

Missing features:
1. Rate limiting - 
2. Auth - CloudTrailMessageBroker - implemented as in-mem dictionary of concurrent queues. The dictionary is registered in the DI as singleton, so each API thread will have the same dictionary instance.
The dictionary isn't concurrent, but is only read during runtime after initalization. And the queues are concurrent.


Test:
Beginning:
1. Look at mongodb, see no db exists (besides the build in ones)
2. Look at the docker logs for the setup, see that anomalydetection-1 is reporting Anomaly1, Anomaly2, and Anomaly3 topics are empty.
3. See that cloudtrailmessagebroker reports that the Dequeue operation is called repetedly and the queues are empty.

Sanity: Post a new CloudTrail event:
1. Go to http://localhost:5255/swagger/index.html, the cloudTrailIngstor API swagger. Post a new message. The default swagger values are ok.
2. See in the logs that cloudtrailingestor is performing PostCloudTrailAsync with the cloudTrail details, which is to post the cloudTrail message onto the message broker.
3. See that cloudtrailingestor is adding the cloudTrail id to its cache.
4. See in the logs that the cloudtrailmessagebroker is reporting EnqueueAsync with the details of the CloudTrail.
5. See that anomalydetection is reporting it got cloudTrail events on Anomaly1, Anomaly2 and Anomaly3
6. See on mongoDB that a new db and collection were created, named AnomalyDetectionResult. See 3 new anomalies results in the colleection.

Check dedup on the Cloudtrail API level:
1. Using the swagger, post the same cloudTrail event again.
2. See that cloudtrailingestor reports it was "Found in cache"
3. See that cloudtrailmessagebroker does not report an EnqueueAsync operation.
4. See that anomalydetection does not report getting a new cloudTrail event.
5. See no new events in the db.

Check dedup in the db level (needed as cache on the cloudTrail api is limited to 1hr)
1. Go to http://localhost:5042/swagger/index.html, where you can push cloudTrail events directly to the message broker, with the filtering done on the cloudTrail api using local cache
2. Using EnqueueAsync in the message broker swagger, post the same cloudTrail even you posted using the cloudTrail api swagger.
3. See that message broker reports EnqueueAsync, then DequeueAsync, so the event was placed on the message broker and taken from there by the anomalydetection service.
4. See anomalydetection reports AlreadyExistsInDB: True
5. See no new results in the db