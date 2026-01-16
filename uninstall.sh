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

# Only remove the symlink if it points to our app
if [ -L "$TARGET" ] && [ "$(readlink "$TARGET")" = "$SOURCE" ]; then
    rm -f "$TARGET"
fi

rm -rf "$SOURCE_DIR"
