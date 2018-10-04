# Datagrid.Net
Engine for dynamically filter/sort using EF

# Usage sample 
By default this will accept Datatables.net parameters format, you can change this by creating your own 
ParameterManager and override Process method.
By default cache is disabled but can be enabled globally setting DatagridSettings.DefaultCacheTimeoutInSeconds 
to 0 (never expires) or higher, you can also enable cache individually calling .SetCache after Process or ModifyData.

~~~~
using LinqKit; //excelent extensions to EF. I use it to create reusable projections

[HttpGet]
public object GetData()
{
    var sampleProjection = Person.SampleProjection; //reference is needed in same scope for linqkit invoke to work
    
    var query = entities
        .Person
        .Where(o => o.Id > 0) //just sample
        .AsExpandable() //need to invoke this method if you pretend to use linqkit extension
        .Select(o => new
        {
            Name = o.Name,
            Environment = sampleProjection.Invoke(o)
        })
        .OrderByDescending(o => o.Name);

    var data = DatagridManager
        .Instance
        .Process(query, Request.Params) //process the filters/sort on database
        .ModifyData(o => o.Select(p => new ResultModel //this is not required unless you need to change something in memory
        {
            Name = SampleMethod(o.Name),
            Environment = SampleMethod(o.Environment)
        }));
    
    return data;
}

public class Person {
    public long Id { get;set; }
    public string Name { get;set; }
    public string SampleProjection { get;set; }
    
    public static Expression<Func<Person, string>> SampleProjection = o => o.SampleProjection == "debug" ? "testing" : "production";
}

public string SampleMethod(string text) {
    return text;
}
~~~~