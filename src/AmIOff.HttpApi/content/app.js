$(function(){

	console.log("JS is ready!")


// results templates
var $resultsOn = $('#results-on');
var _resultsOn = _.template($('#resultsOn-template').html());

var $resultsOff = $('#results-off');
var _resultsOff = _.template($('#resultsOff-template').html());


var $heading = $('#heading');
var _heading = _.template($('#heading-template').html());




// On page load
$("#datepicker" ).datepicker({ 
	minDate: 0, 
	maxDate: 30 
});

$('#timepicker').timepicker({
	step: 60,
	disableTouchKeyboard: true

});

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
	$heading.empty();
	$resultsOn.empty();
	$resultsOff.empty();
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
	console.log("Date with timezone: "+dateTimeZone)

	localStorage.setItem("program-name", $programName);
	var storedValue = localStorage.getItem("program-name");
	console.log(storedValue);
	// Pick either the date from form. If no date entered, select current time
	
	var unixtime = Date.parse(dateTimeZone).getTime()/1000; 

	console.log("Unix-timezone being sent to server: "+unixtime)
	
	
	


	

	$.get('/api/'+$programName+'/'+unixtime,
	  	function(data) {

	  		// var names = [
	  		// {firstName: "Manu", lastName: "Lohiya"},
	  		// {firstName: "Neil", lastName: "Maheshwari"}

	  		// ];
	  		
	  		console.log("Sample data being returned by server[0]: " , data[0])	

	  		var endOfMonth = [1454284800, 1456790400, 1459382400, 1462060800, 1464739200, 1467331200];
	  		
	  		_.each(data, function (name, index) {
	  			var date = moment.unix(name.timeFreeUntil).format("MM/DD/YYYY")
	  			var time = moment.unix(name.timeFreeUntil).format("h a")
	  			


	  			var offScheduleFlag = isInArray(name.timeFreeUntil, endOfMonth);
	  			
	  			
	  			var nameObject = {firstName : name.firstName, lastName : name.lastName, date : date, time : time, offScheduleFlag: offScheduleFlag}
	  			var $name1 = $(_resultsOn(nameObject));
	  			$name1.attr('data-index', index);
	  			var $name2 = $(_resultsOff(nameObject));
	  			$name2.attr('data-index', index);
	  			$resultsOn.append($name1);
	  			$resultsOff.append($name2);
	  		});
	  		
	
		}	
	);

});










});





// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300