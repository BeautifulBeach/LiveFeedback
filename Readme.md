# LiveFeedback explained

LiveFeedback is a program that allows learners to give real-time feedback on comprehensibility during a lecture. The lecturer/teacher starts the main program on the PC on which the presentation is being given, a QR code can be scanned by the audience, which takes them to a web portal from which they can use a slider to adjust the comprehensibility from “too easy” to “just right” to “too difficult”.



## For administrators and developers

### LiveFeedback consists of several components:

- The desktop program, which effectively serves as a central point for the presenter. From here, settings can be defined and the option to send in evaluations can be started/terminated. It is also responsible for displaying an overlay above all other windows. This currently only works under Windows, but not under some Linux desktops (KDE). Mac has not been tested. For those Linux desktops and possibly MacOs, an extension for the presentation program would have to be created that communicates with the main program. This is not currently planned.
- A web frontend: This is the web interface that listeners will see on their devices when they scan the QR code or enter the URL manually. It automatically connects to the server (even if the connection is interrupted by any standbys) and sends the new value to the server as soon as the slider is moved. If executed in monolithic mode, this is included in the main program.
- A server: This listens on port 5000 and serves as the central communication interface between the desktop program and the web front end. It uses SignalR under the hood to enable redundant real-time communication, so make sure that WebSockets are allowed if the server is running externally and not on the same host as the main program. If executed in monolithic mode, the server is included in the main program.
- A shared class library that contains shared types and functions

## Ways to deploy it:

LiveFeedback can run in two ways:

- monolithic:
  In monolithic mode, all components, i.e. the main program, the server and the front end, run on the same computer in the same process. This mode is preferred because it is easy to set up and less prone to errors. Restriction: The evaluation only works locally and the listeners must be in the same network as the PC on which everything is running.
- distributed:
  
  The distributed mode solves these problems. In this case, the server with the front end runs on its own server, which can be located anywhere; it only needs to be accessible to the presentation PC and the audience. In this case, only the main program runs on the presentation PC, which connects to the server via SignalR. This mode is potentially less secure and more error-prone and is not recommended.

The web frontend as a wwwroot folder that contains static files. It has to be located on the PC where the server runs.

## Environment variables:

- required: `LIVE_FEEDBACK_MODE=local`  or `distributed`  specifies whether the components should assume that they are running locally or distributed. In case of distributed, the server parts assume that they are running in a docker container and default to the current working directory where they assume the wwwroot folder

- it depends: `LIVE_FEEDBACK_SERVER_HOST`: If you are using distributed mode, this is required. Set it to the correct host on the presentation PC and on the server. If it is not set correctly, the audience will not be able to access the correct web interface via the QR code. If you are using local mode, the program will always try to use a local IP of the current network that can be reached by the clients. If the presentation PC is connected to several networks at the same time or if you prefer a local domain instead of an IP address, you should set this environment variable as a precaution.

- it depends: `LIVE_FEEDBACK_ENVIRONMENT`  set to `dev` or `development` means that the compontents assume they are running in development mode, everything else will default to production mode.

- optional: `LIVE_FEEDBACK_WWWROOT`: LiveFeedback always tries to determine the wwwroot folder automatically based on the other two environment variables, but if it fails for some reason, you can use this to manually set the absolute path to the wwwroot folder.

- optional: `LIVE_FEEDBACK_SERVER_PORT` is used if you want or need to use a port other than the default port (5000).
