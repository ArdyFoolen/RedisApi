# RedisApi

###### Redis-cli installation on Windows

1. [Visit the archived MSOpenTech Redis Github repository](https://github.com/MicrosoftArchive/redis/).
2. Scroll down to the “Redis on Windows” section and click on the release page link.
3. Find the latest version (currently 3.2.100).
4. Download and run the .msi file and walk through the Setup Wizard instructions. Accept the Wizard’s default values, but make sure to check the “Add the Redis installation folder to the Path environment variable” checkbox.
5. Make sure the Redis service is started.

###### Connect to instance

1. Open your Command Prompt (ex: cmd.exe).
2. Run the following command: redis-cli ping
