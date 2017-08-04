ThumbCached is a simple, high-performance, distributed caching and storing  server for web site.

It's commonly used to store large number small capacity data that 
read frequently, such as image thumbnails, user custom face, formatted text etc.

ThumbCached can be used as memory cache service as well, and it can reach 70% speed of Memcached.

Start in Windows
----
1. Build the solution.
2. Copy "{project dir}\lib\sqlite3.dll" to the build-output directory.
3. Run "ThumbCached.exe".



Start in Linux
---
1. Build the project with Mono.
2. Copy "{project dir}/lib/sqlite-3.6.10.so" to "/usr/lib", 
   and then create or update symbol links "libsqlite.so.0" and "libsqlite3.so.0" set 
   them refer to "/usr/lib/sqlite-3.6.10.so"
3. Run "Mono ThumbCached.exe".



Install ThumbCached as windows Service
----
1. Build the solution.
2. Copy "{project dir}\lib\sqlite3.dll" to the build-output directory.
3. Go to the build-output directory and run "InstallService.bat".
4. Run "StartService.bat" to start service.
5. Run "StopService.bat" can stop service.
6. Run "UninstallService.bat" can uninstall this service.
