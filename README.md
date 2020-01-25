# NBloodServerSupervisor
Web API and NBlood server launcher service in .NET Core

### Purpose
Not everybody is able to open ports and host games. If you run this container on a VPS, anybody can play, because this will host NBlood servers automatically all the time! It also functions as a server browser, people will be able to see these servers and hop into one of them and wait until it gets full and then the game will start. (It is advised to start the game in windowed mode so you can do other stuff while you wait, when the game is started, you can switch back to fullscreen.) It is still not possible to join if the game has already started! But people can join to another server and wait until it gets full and after this the game will be eventually started.

### How it works
After you start the container, the following will happen:

6 NBlood servers will be started, each will have a different number of maximum players, ranging from 3 to 8. Each server will have a player named `SERVER` which won't do anything, you can kill it and it will never respawn. If people open the `listservers` URL, they will see the ports for all the servers - and to make it easier for them - they will also see the command line what they can just copy and paste into a command prompt if they want to join. They will also see how many players have already joined. The game will only start if the required (maximum) number of players have joined. In that case, a new server will be started automatically, so other people can play too. Let's say 3 people joined to the 3 player server, then a new 3 player server will be launched automatically. It is possible to request private servers with the `startserver` URL with the desired number of required players. These servers are not listed by `listservers`. In the response of `startserver` you will get the port number for that server. Not anybody can start a private server, people need to know the API key for that.

## Debug with Visual Studio
1. Publish `WebInterface` to a folder with Debug configuration
2. Create a new folder in the publish folder and name it `blood`
3. Compile NBlood with `make blood NORENDER=1` or with `mingw32-make blood NORENDER=1`
4. Copy `nblood.exe` into the new `blood` folder and rename it to `nblood_server.exe`
5. Copy the following files of Blood 1.21 into the `blood` folder:
- BLOOD.INI
- BLOOD.RFF
- GUI.RFF
- SOUNDS.RFF
- SURFACE.DAT
- TILES000.ART-TILES017.ART
- VOXEL.DAT
6. Start `WebInterface.exe`
7. Attach the debugger to `WebInterface.exe` and/or `Supervisor.exe`
8. You can call the following URLs with your web browser or Postman:
- http://localhost:5000/nblood/api/listservers
- http://localhost:5000/nblood/api/startserver?players=3&ApiKey=CHANGEME

## Deploy the server onto GNU/Linux
1. Install Docker and wget (if you don't have already), for example like this: `sudo snap install docker && sudo apt install wget -y`
2. Download the Dockerfile: `wget https://raw.githubusercontent.com/CommonLoon102/NBloodServerSupervisor/master/Dockerfile --directory-prefix=supervisor`
3. Build the Docker image: `sudo docker build -t nblood-supervisor:latest supervisor`
4. Navigate to your Blood 1.21 directory where you have these files:
- BLOOD.INI
- BLOOD.RFF
- GUI.RFF
- SOUNDS.RFF
- SURFACE.DAT
- TILES000.ART-TILES017.ART
- VOXEL.DAT
5. Run a Docker container from there: `sudo docker run --volume "$PWD":/supervisor/publish/blood --network=host --detach nblood-supervisor`
6. Optional: You can see the ApiKey here:
- `sudo docker run -it nblood-supervisor /bin/bash`
- `cat /supervisor/publish/appsettings.json | grep 'ApiKey'`
- `exit`

## Deploy the server onto Windows Server 2016 or newer
This is not tested at all, but it should be possible, because it is possible to run Docker on Windows Server 2016 or newer. Therefore the steps would be very similar as above:
1. Download the `Dockerfile` from here: `https://raw.githubusercontent.com/CommonLoon102/NBloodServerSupervisor/master/Dockerfile`
2. Build the Docker image based on the Dockerfile, you should use PowerShell I guess: `docker build -t nblood-supervisor:latest .`
3. Navigate to your Blood 1.21 directory (step #4 from above).
4. Run the container, I don't know the command, should be something like this, open a PR if you know: `docker run --volume $((pwd).Path):/supervisor/publish/blood --network=host --detach nblood-supervisor`

## Usage
You can list the currently running public servers via this URL:

http://your.ip.goes.here:23580/nblood/api/listservers

You can start new private servers via this URL:

http://your.ip.goes.here:23580/nblood/api/startserver?players=3&ApiKey=the_actual_apikey_here

The number of players must be at least 3 and maximum 8. The servers started with this URL won't be visible publicly via the `listservers` URL. You can see the port and the command line command to join in the response.
