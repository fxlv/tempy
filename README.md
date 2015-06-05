# Tempy
Store and display weather data

## Setting up
clone the git repo, then in the cloned directory do
```
npm install
```
This will download all the dependencies.

Now you can try running the ```start.sh``` script and see what happens :)

## Populating the weather data
Update temperature using curl like so

```
curl -XPOST http://tempy.hostname.here/add -d 'sensor=sensor_name_here&temperature=temperature_reading'
```
