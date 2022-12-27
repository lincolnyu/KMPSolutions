// See https://aka.ms/new-console-template for more information

using KmpCrmCore;

var src = args[0];
var dst = args[1];

var v1 = new CsvSerializerV1();
using var sr = new StreamReader(src);
using var sw = new StreamWriter(dst);

var crm = v1.Deserialize(sr);
var vc = new CsvSerializer();
vc.Serialize(crm, sw);