# Tempy
Store and display weather data

## Populating the weather data
Update temperature using curl like so

```
curl -XPOST http://tempy.hostname.here/add -d 'sensor=sensor_name_here&temperature=temperature_reading'
```
