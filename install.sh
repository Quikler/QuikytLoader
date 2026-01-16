#!/bin/bash
# Export config.sh with common variables (better to export it before '-u' option or before variable use)
source "$(dirname "$0")/config.sh"

# '-e' exits on error to prevents scripts from silently continuing after a failure, '-u' exits on unset variables (protecting rm -rf /$APP_NAME if $APP_NAME is empty), and '-o pipefail' ensures errors in pipelines are caught.
set -euo pipefail

if [[ "$EUID" -ne 0 ]]; then
  echo "This script must be run as root" 
  exit 1
fi

SOURCE_DIR="/opt/$APP_NAME"
SOURCE="$SOURCE_DIR/$APP_NAME"
TARGET="/usr/bin/$APP_NAME"

RID=${RID:-linux-x64}
TFM=${TFM:-net9.0}

# But before make sure to run 'make trimmed RID=YOUR_ARCHITECTURE' for trimmed app OR 'make aot RID=YOUR_ARCHITECTURE' for AOT app
# YOUR_ARCHITECTURE=win-x64,linux-x64 etc
mkdir -p /opt/$APP_NAME/
cp ./$APP_NAME.Startup/bin/Release/$TFM/$RID/publish/* "$SOURCE_DIR"

# Only remove if it’s a symlink pointing to our app (safer than --force)
if [ -L "$TARGET" ] && [ "$(readlink "$TARGET")" = "$SOURCE" ]; then
    rm "$TARGET"
fi

ln -s "$SOURCE" "$TARGET"
