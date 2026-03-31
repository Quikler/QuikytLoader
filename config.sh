APP_NAME=QuikytLoader
TFM=net10.0

if [[ -z "$APP_NAME" ]]; then
  echo "Error: APP_NAME is empty or not set in config.sh. Aborting." >&2
  exit 1
fi

export APP_NAME TFM
