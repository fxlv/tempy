// dependencies
try {
	var express = require("express");
	var bodyParser = require("body-parser");
	var mongoose = require("mongoose");
} catch (err){
	console.error("Failed loading one of the dependencies");
	console.error("Please make sure you have ran 'npm install'");
	console.error("See the following message for more details:");
	console.error(err);
}

var Temperature = require("./temperature");
try {
	var creds = require("./creds");
} catch (err){
	console.error("Credentials missing");
	console.error("Please provide 'creds.js' file. See documentation for details.");
}
// setting up the app
var app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended:true}));


var db = mongoose.connection;
db.on('error', console.error.bind(console, 'connection error:'));
db.once('open', function (callback) {
	console.log("Connected to mongo");
});
mongoose.connect(creds.mongodb_uri);

// serve static files from default root
app.use("/", express.static("static"));

app.get("/get/:sensor/last", function(req,res){
	console.log(req.params);
	if(req.params.sensor){
	Temperature.find({sensor:req.params.sensor}).sort({created:-1}).limit(1).exec(function(err, temperatures){
		if(err){
			console.log("Error ocurred");
			console.log(err);
			res.json({"status":"failed"});
			return;
		}
		res.json(temperatures[0]);
	});
	console.log("/get called");
	} else {
		res.json({"status":"fail"});
	}
});


app.get("/get/:sensor/all", function(req,res){
	console.log(req.params);
	if(req.params.sensor){
	Temperature.find({sensor:req.params.sensor}, function(err, temperatures){
		if(err){
			console.log("Error ocurred");
			console.log(err);
			res.json({"status":"failed"});
			return;
		}
		res.json(temperatures);
	});
	console.log("/get called");
	} else {
		res.json({"status":"fail"});
	}
});

app.get("/count", function(req,res){
	Temperature.find({}).count().exec(function(err, count){
		if(err){
			console.log("Error ocurred");
			console.log(err);
			res.json({"status":"failed"});
			return;
		}
		res.json({"item_count":count});
	});
	console.log("/count called");
});

app.get("/sensors", function(req,res){
	Temperature.find().distinct("sensor", function(err, sensors_list){
		console.log("Sensors callback started");
		if(err){
			console.log("Error ocurred");
			console.log(err);
			res.json({"status":"failed"});
			return;
		}
		res.json({"sensors_list":sensors_list});
		console.log("Sensors callback ended");
	});
	console.log("/sensors called");
});

app.post("/add", function(req, res){
	res.json(req.body);
	console.log("Got a request to /add");
	console.log(req.body);
	var humidity = "";
	var pressure = "";
	var wind = "";
	
	if(req.body.source && req.body.sensor && req.body.temperature){
		if(req.body.humidity){
			humidity = req.body.humidity;
		}
		console.log("Got all the required data, ready for insert.");
		var temp = new Temperature({
			source: req.body.source,
			sensor: req.body.sensor,
			humidity: humidity,
			pressure: pressure,
			wind: wind,
			temperature: req.body.temperature,
			created: new Date()
		});
		temp.save(function (err, data ){
	if(err){
		console.log("Error while saving!!!");
	} else {
		console.log("Saved!");
		console.log(data);
	}
});
	}
});

app.listen(3000);
