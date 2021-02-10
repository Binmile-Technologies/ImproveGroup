function checkYearforCorpGNA(executionContext) {
    debugger;
    var formContext = executionContext.getFormContext();

    var date = formContext.getAttribute("ig1_year").getValue();

    if (date != null) {

        var year = date.getFullYear();
        var fetchData = {
            ig1_year: year
        };
        var fetchXml = [
    "<fetch>",
    "  <entity name='ig1_corpgna'>",
    "    <attribute name='ig1_corpgnavalue' />",
    "    <attribute name='ig1_year' />",
    "    <attribute name='ig1_corpgnaid' />",
    "    <filter>",
    "      <condition attribute='ig1_year' operator='in-fiscal-year' value='", fetchData.ig1_year/*2020*/, "'/>",
    "    </filter>",
    "  </entity>",
    "</fetch>",
        ].join("");

        var corpgna = executeFetchXml("ig1_corpgnas", fetchXml);

        if (corpgna.value.length > 0) {

            alert("CorpGNA Year Already Exist Please change value of CorpGNA");
            formContext.getAttribute("ig1_year").setValue(null);
            return;
        }

       }
    
      
    }
