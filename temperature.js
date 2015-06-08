var mongoose = require("mongoose");

var Schema = mongoose.Schema;

var temperatureSchema = new Schema({
	source: String,
	sensor: String,
	temperature: String,
	humidity: String,
	wind_speed: String,
	wind_direction: String,
	pressure: String,
	created: Date
});

module.exports = mongoose.model("Temperature", temperatureSchema);
