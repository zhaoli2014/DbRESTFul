
echo "%0"
echo "%1"
cd "%1"

taskkill /F /IM dotnet.exe

dotnet run

@pause