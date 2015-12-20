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
	$results.empty();
	var $programName = $('#program-name').val();
	var $date = $('#date').val();
	console.log("Program Name: "+$programName)
	console.log("Date: "+$date)
	
	// $.get('url+$programName+ $date',
	   var names = [
  {firstName: "Manu", lastName: "Lohiya"},
  {firstName: "Neil", lastName: "Maheshwari"}

];
	   // var names = 
	   // {
    //     firstName: "Manu",
    //     lastName: "Lohiya"
    // 	};

    // 	var name2 = 	
    // 	{
    // 	firstName: "Neil",
    //     lastName: "Maheshwari"
    // 	};
    
    // 	var data = [name1,name2];
    	console.log(names)	
		
    	_.each(names, function (name, index) {
  		var $name = $(_results(name));
  		$name.attr('data-index', index);
  		$results.append($name);
		});

		
});




$('#datetimepicker').datetimepicker({
    format: 'MM/dd/yyyy hh:mm',
    language: 'en',
    pickSeconds: false, 
    pick12HourFormat: true
});

});





// "Chang, Julia",2529,922,"UCSF-EM 3p-11p (Blue)",429,124,12-1-15,1500,2300