function test()
{
var entityName = Xrm.Page.data.entity.getEntityName();
alert(entityName);
var id =  Xrm.Page.data.entity.getId();
alert(id);
}