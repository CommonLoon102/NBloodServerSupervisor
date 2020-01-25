# NBloodServerSupervisor
Web API and NBlood server launcher service in .NET Core

## Debug with Visual Studio
1. Publish `WebInterface` to a folder with Debug configuration
2. Create a new folder in the publish folder and name it `blood`
3. Compile NBlood with `make nblood NORENDER=1` or with `mingw32-make nblood NORENDER=1`
4. Copy nblood.exe into the new `blood` folder and rename it to `nblood_server.exe`
5. Copy the following files of Blood 1.21 into the `blood` folder:
- BLOOD.INI
- BLOOD.RFF
- GUI.RFF
- SOUNDS.RFF
- SURFACE.DAT
- TILES000.ART-TILES017.ART
- VOXEL.DAT
6. Start WebInterface.exe
7. Attach the debugger to WebInterface.exe and/or Supervisor.exe
8. You can call the following URLs with your web browser or Postman:
- http://localhost:5000/nblood/api/listservers
- http://localhost:5000/nblood/api/startserver?players=3&ApiKey=CHANGEME

## Deploy the server onto GNU/Linux
1. Install Docker and wget (if you don't have already), for example like this: `sudo snap install docker && sudo apt install wget -y`
2. Download the Dockerfile: `wget https://raw.githubusercontent.com/CommonLoon102/NBloodServerSupervisor/master/Dockerfile --directory-prefix=supervisor`
3. Switch to the new `supervisor` directory: `cd supervisor`
4. Build the Docker image: `sudo docker build -t nblood-supervisor:latest .`
5. Navigate to your Blood 1.21 directory where you have these files:
- BLOOD.INI
- BLOOD.RFF
- GUI.RFF
- SOUNDS.RFF
- SURFACE.DAT
- TILES000.ART-TILES017.ART
- VOXEL.DAT
6. Run a Docker container from there: `sudo docker run --volume "$PWD":/supervisor/publish/blood --network=host --detach nblood-supervisor`
7. Optional: You can see the ApiKey here:
- `sudo docker run -it nblood-supervisor /bin/bash`
- `cat /supervisor/publish/appsettings.json | grep 'ApiKey'`
- `exit`

## Usage
You can list the currently running public servers via this URL:

http://your.ip.goes.here:23580/nblood/api/listservers

You can start new private servers via this URL:

http://your.ip.goes.here:23580/nblood/api/startserver?players=3&ApiKey=the_actual_apikey_here

The number of players must be at least 3 and maximum 8. The servers started with this URL won't be visible publicly via the `listservers` URL. You can see the port and the command line command to join in the response.
