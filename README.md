
# ION - Incredible Object Notation #

### What is ION? ###

ION is an alternative to JSON providing smaller message sizes and faster serialization and deserialization for sending messages over a network. ION is smaller because it cuts out the headers for fields that you would normally find in JSON, it uses the order of the fields and properties in a model to serialize and deserialize. ION started as a solution to minimizing the size of sending models across the internet whilst eliminating the need to code every time a model is changed.

### How do I get set up? ###

* Download Repo
* Build ION project
* Reference ION.dll
* Add using IONConvert;
* To serialize simply use: 
```
#!c#
byte[] ion = ION.SerializeObject(model, structureParity: false, compression: false);
```
* To deserialize simply use:
```
#!c#
 Model model = ION.DeserializeObject<Model>(modelbytes, structureParity: false, compression: false);
```

### Size ###

Compared to JSON, tests show a message size decrease of 348% and 556% with compression.

### Speed ###

Compared to JSON, tests show a serialization speed increase of 2286% and a deserialization speed increase of 1413%

### Features ###

* Quick and Small Serialization and Deserialization
* Byte and String Serialization and Deserialization
* Many supported data types (custom classes, [], List<>, bool, byte, ushort, short, uint, int, ulong, long, float, double, decimal, enums, etc.)
* Data Parity Check
* Structure Parity Check
* Bool compression - multiple bools (up to 8) use only 1 byte
* GZIP Compression (Still faster than JSON)
* And continually adding more!

### Future ###

I have set up the basics, and ION is still in early development stages today (21/06/2017), but I have very ambitious plans for it and believe it has some very useful applications. Especially where flexibility can be compromised for speed and size. Proposed features are kept in the issue tracker.

### Limitations ###

Of course, speed and size advantages introduces some limitations. Great power comes with great responsibility! For example, changing the order of fields and properties in a model breaks deserialization in clients. This can however of course be overcome. ION comes with parity check as standard which can be enabled at a cost of 64 bytes per message, to make sure messages are deserialized with the same models.

### Contact ###

* If you have any ideas or issues, the issue tracker is your friend. Alternatively please email me (Corne Vermeulen) on Nexum1@live.com
