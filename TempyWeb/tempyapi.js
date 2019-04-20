$( function(){
    $("#status").load("http://localhost:5000/status");

    var tempData = [];
    $("#tempdata").hide();


    $.getJSON("http://localhost:5000/api/measurements/names", function (data) {
        $("#querystatus").text("Getting station names");
        console.log(data);
        $.each(data, function (index, stationName) {

            

            $.getJSON("http://localhost:5000/api/measurements/"+stationName).then(function (stationData) {
                tempData.push({name: stationName, temperature: stationData.value});
                console.log(tempData);
                $("#querystatus").text("Got data for "+stationName);


            });
            

        })

    });
  
       setTimeout(function(){

               $("#querystatus").text("All done");


               $.each(tempData, function(key,value){
                console.log(key+" - "+value);
               $("#tempdata").append("<tr><td>"+value.name+"</td><td>"+value.temperature+"</td></tr>");
               $("#tempdata").show();
            })}, 
       6000);
       

  







});