# RedisApi

## Prerequisites

###### Redis service and Redis-cli installation on Windows

1. [Visit the archived MSOpenTech Redis Github repository](https://github.com/MicrosoftArchive/redis/).
2. Scroll down to the “Redis on Windows” section and click on the release page link.
3. Find the latest version (currently 3.2.100).
4. Download and run the .msi file and walk through the Setup Wizard instructions. Accept the Wizard’s default values, but make sure to check the “Add the Redis installation folder to the Path environment variable” checkbox.
5. Make sure the Redis service is started.

###### Connect to instance

1. Open your Command Prompt (ex: cmd.exe).
2. Run the following command: redis-cli ping

###### Using Redis-cli

1. In the Command Prompt run: redis-cli -h 127.0.0.1 -p 6379
2. For an introduction on Redis [Click Here](https://redis.io/topics/data-types-intro)

###### Another user-friendly UI tool for browsing

1. Visit [Redis React](https://github.com/ServiceStackApps/RedisReact).
2. Scroll down to Download and Windows and click RedisReact-winforms.exe

###### Re-install StackExchange.Redis

1. In Visual Studio in menu Tools > NuGet Package Manager > Manage NuGet Packages for Solution
2. In tabblad Browse search for StackExchange.Redis
3. Select RedisApi and Uninstall
4. After in Version make sure 1.2.6 is selected and click Install

*Note:* Do not select a newer version, because the multiplexer will throw a RedisConnectionException on the async methods even after the check that it is connected. 
