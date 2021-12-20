@ECHO OFF
ECHO Batch file was executed successfully.
wmic process where name='EmergenceEVMLocalServer.exe' delete
start %~dp0\Server\EmergenceEVMLocalServer.exe --walletconnect={\"Name\":\"Crucibletest\"^,\"Description\":\"UnrealEngine+WalletConnect\"^,\"Icons\":\"https://crucible.network/wp-content/uploads/2020/10/cropped-crucible_favicon-32x32.png\"^,\"URL\":\"https://crucible.network\"}
