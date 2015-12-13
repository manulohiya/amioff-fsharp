$(document).ready()

  console.log("JS is ready!")

  $('#datetimepicker').datetimepicker({
        format: 'MM/dd/yyyy hh:mm',
        language: 'en',
        pickSeconds: false, 
        pick12HourFormat: true
      });

