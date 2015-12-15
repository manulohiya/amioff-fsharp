$(document).ready()

  console.log("JS is ready!")


$("#program-search").submit(function(event) {
	console.log("Submit button is working")
	event.preventDefault();
	var $programName = $('#program-name').val();
	console.log("Program Name: "+$programName)
	
});




$('#datetimepicker').datetimepicker({
    format: 'MM/dd/yyyy hh:mm',
    language: 'en',
    pickSeconds: false, 
    pick12HourFormat: true
});




// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300