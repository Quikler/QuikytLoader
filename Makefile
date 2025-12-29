# IN OUR CASE
# aot: 53.9 MB
# trimmed: 25.1 MB

# TRIMMED ONLY (slower startup, less size, JIT at runtime)
trimmed:
	dotnet publish QuikytLoader.Startup -r linux-x64 -c Release -p:PublishTrimmed=true -p:TrimMode=full

# NATIVE AOT (bigger size, faster startup)
aot:
	dotnet publish QuikytLoader.Startup -r linux-x64 -c Release -p:PublishAot=true -p:OptimizationPreference=Size

clean:
	find . -type d \( -name "bin" -o -name "obj" \) -exec rm -rf {} +
