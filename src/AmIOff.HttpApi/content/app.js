$(function(){

	console.log("JS is ready!")


// results template
var $results = $('#results');
var _results = _.template($('#results-template').html());


// On page load



// On submit button
$("#program-search").submit(function(event) {
	console.log("Submit button is working")
	event.preventDefault();
	
	var $programName = $('#program-name').val();
	var $date = $('#date').val();
	var unixtime = Date.parse($date).getTime()/1000
	console.log("Program Name: "+$programName)
	console.log("Date: "+$date)
	console.log("Unix: "+unixtime)
	
	

	$.get('/api/'+$programName+'/'+unixtime,
	  	function(data) {
	  		$results.empty();
	  		// var names = [
	  		// {firstName: "Manu", lastName: "Lohiya"},
	  		// {firstName: "Neil", lastName: "Maheshwari"}

	  		// ];
	  		
	  		console.log("NAMES: " + data)	
	  		
	  		_.each(data, function (name, index) {
	  			var $name = $(_results(name));
	  			$name.attr('data-index', index);
	  			$results.append($name);
	  		});

		}	
	);

});






$('#datetimepicker').datetimepicker({
	format: 'MM/dd/yyyy hh:mm',
	showMeridian: true,
	language: 'en',	
	pickSeconds: false, 
	pick12HourFormat: true
	

});



});





// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300