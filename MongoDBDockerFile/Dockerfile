# Use the official MongoDB image
FROM mongo:latest

# Set the MongoDB port to 27018
ENV MONGO_INITDB_ROOT_USERNAME=mongoadmin
ENV MONGO_INITDB_ROOT_PASSWORD=secret

# Expose the modified MongoDB port
EXPOSE 27018

# Start MongoDB with the modified port
CMD ["mongod", "--port", "27018", "--auth"]
