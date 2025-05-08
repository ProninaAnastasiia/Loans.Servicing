#!/bin/bash

set -e

echo "⏳ Waiting for Kafka to be ready..."
sleep 10  # Можно заменить на healthcheck, но это проще

TOPICS=(
  create-contract-requested
  calculate-contract-values
  calculate-indebtedness
  update-contract-requested
)

for topic in "${TOPICS[@]}"
do
  echo "📌 Creating topic: $topic"
  /opt/kafka/bin/kafka-topics.sh \
    --create \
    --if-not-exists \
    --topic "$topic" \
    --partitions 3 \
    --replication-factor 2 \
    --bootstrap-server kafka1:19092
done