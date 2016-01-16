$(function(){

	console.log("JS is ready!")


// results template
var $results = $('#results');
var _results = _.template($('#results-template').html());

var $heading = $('#heading');
var _heading = _.template($('#heading-template').html());

// On page load
$("#datepicker" ).datepicker({ 
	minDate: -60, 
	maxDate: 60 
});

$('#timepicker').timepicker({
	step: 60,
	disableTouchKeyboard: true

});


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
	$heading.empty();
	event.preventDefault();
	
	


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
	console.log("Date: "+$date)
	console.log("Time: "+$time)
	console.log("TimeZone: "+$timeZone)
	console.log("Date with timezone: "+dateTimeZone)




	// Pick either the date from form. If no date entered, select current time
	
	var unixtime = Date.parse(dateTimeZone).getTime()/1000; 

	console.log("Unix-timezone: "+unixtime)
	
	
	


	

	$.get('/api/'+$programName+'/'+unixtime,
	  	function(data) {
	  		$results.empty();
	  		// var names = [
	  		// {firstName: "Manu", lastName: "Lohiya"},
	  		// {firstName: "Neil", lastName: "Maheshwari"}

	  		// ];
	  		
	  		console.log("NAMES: " + data)	
	  		
	  		_.each(data, function (name, index) {
	  			var date = moment.unix(name.timeFreeUntil).format("MM/DD/YYYY")
	  			var time = moment.unix(name.timeFreeUntil).format("h:mm a")
	  			console.log("Date: " + name.date)
	  			var nameObject = {firstName : name.firstName, lastName : name.lastName, date : date, time : time}
	  			var $name = $(_results(nameObject));
	  			$name.attr('data-index', index);
	  			$results.append($name);
	  		});

		}	
	);

});










});





// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300