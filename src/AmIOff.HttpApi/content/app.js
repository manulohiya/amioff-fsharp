$(function(){

	console.log("JS is ready!")


// results templates
var $resultsOn = $('#results-on');
var _resultsOn = _.template($('#resultsOn-template').html());








// On page load
$("#datepicker" ).datepicker({ 
	minDate: 0, 
	maxDate: 30 
});

$('#timepicker').timepicker({
	step: 60,
	disableTouchKeyboard: true

});

$('#heading-on').hide();
$('#heading-off').hide();

  // Retrieve the users program-name
  var name = localStorage.getItem('program-name');
  console.log("program-name: ", name)
  if (name != "undefined" || name != "null") {
  	$("#program-name").val(name);
  } else {

  	$("#program-name").val("e.g. UCSFEMx`");
  }


  function isInArray(value, array) {
  	return array.indexOf(value) > -1;
  }

  var dateChecker = function(date) {
  	console.log("dateChecker function is working")
  	var newDate = date;
  	var myRe = /.+?(?=\/)/;
  	var myArray = myRe.exec(date);
  	var myRe2 = /.{4}$/;
  	var myArray2 = myRe2.exec(date);
  	var month = parseInt(myArray);
  	var year = parseInt(myArray2);


  	if (month <= 6) {
  		console.log("Month is between 1 and 6");	
  		year = year - 1;
  		yearstr = year.toString()
  		console.log("Year (after) " + yearstr);
  		var removeYear = date.slice(0, - 4);
  		newDate = removeYear + yearstr;				
  	}
  	return newDate
  };

// var date = new Date(unixTimestamp*1000);

// console.log("UnixTime now: "+timeInMs)
// console.log("Time now: "+date)


// On submit button
$("#program-search").submit(function(event) {
	console.log("Submit button is working")
	// $heading.empty();
	
	
	
	// event.preventDefault();
	
	


	var $programName = $('#program-name').val();
	var $calendarDate = $('#datepicker').val();
	var $date = dateChecker($calendarDate);
	var $time = $('#timepicker').val();
	var $timeZone = $('#timeZone :selected').text();
	var dateTimeZone = $date+" "+$time+" "+$timeZone;

	var timeObject = {time : $time, calendarDate : $calendarDate, timeZone : $timeZone, programName: $programName }
	var $inputTime = $(_heading(timeObject));
	 // $inputTime.attr('data-index', index);
	 $heading.append($inputTime);

	// var value = $('#dropDownId:selected').text()
	// var currentTime = $('#date').val();
	
	console.log("Program Name: "+$programName)
	console.log("Date with timezone: "+dateTimeZone)

	localStorage.setItem("program-name", $programName);
	var storedValue = localStorage.getItem("program-name");
	console.log(storedValue);
	// Pick either the date from form. If no date entered, select current time
	
	var unixtime = Date.parse(dateTimeZone).getTime()/1000; 

	console.log("Unix-timezone being sent to server: "+unixtime)

	$.ajax({
        // url: "https://amioff.work/api/hang?programName=$programName%20time:unixtime",
        url: "/api/hang?programName="+$programName+"&"+"time="+unixtime,
        type: "GET",
        success: function(data) {
        	console.log(data)

            // Find list of students who are free
            _.each(data.staff, function (resident, index) {
            	console.log("Resident", resident)
            	var firstName = resident.staffObject.firstName;
            	var lastName = resident.staffObject.lastName;
            	var date = moment.unix(resident.freeUntil).format("M/D  (ddd)")
            	var freeUntil = moment.unix(resident.freeUntil).format("ha")
            	var grouping = resident.grouping;
            	var staffType = resident.staffObject.staffType;
            	var residentObject = {firstName : firstName, lastName : lastName, date : date, freeUntil : freeUntil, staffType: staffType, grouping: grouping}
            	console.log("Resident Object: ",residentObject)

            	
            	$resultsOn.append(residentObject);

            });
        },
        error: function() {
        	alert("Error!");
        }
    });

	$.ajax({
        // url: "https://amioff.work/api/swap?programName=$programName%20time:unixtime",
        url: "/api/swap?programName=$programName"+"&"+"startTime="+unixtime+"&"+"endTime="+(unixtime+1000)+"&"+"staffType="+"UCSF EM R2",
        type: "GET",
        success: function(data) {
            // console.log(data);



        },
        error: function() {
        	alert("Error!");
        }
    });






});










});





// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300