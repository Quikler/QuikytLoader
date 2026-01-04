# But before make sure to run 'make trimmed RID=YOUR_ARCHITECTURE' for trimmed app OR 'make aot RID=YOUR_ARCHITECTURE' for AOT app
# YOUR_ARCHITECTURE=win-x64,linux-x64 etc
APP_NAME=QuikytLoader
mkdir -p /opt/$APP_NAME/
cp ./$APP_NAME.Startup/bin/Release/net9.0/*/publish/* /opt/$APP_NAME/
ln -s /opt/$APP_NAME/$APP_NAME /usr/bin/$APP_NAME --force
