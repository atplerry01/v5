#!/bin/sh
set -e

mc alias set local http://minio:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD"

for BUCKET in documents evidence audit; do
  mc mb --ignore-existing "local/$BUCKET"
  echo "Bucket '$BUCKET' ready."
done

echo "All buckets created."
