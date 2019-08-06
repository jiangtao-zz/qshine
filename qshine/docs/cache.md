# Cache Service

Cache service provides a unified API to access different pluggable cache components.
Add cache components to the application through application environment configure setting.

The common cache components:
  - MemoryCache - https://docs.microsoft.com/en-us/dotnet/api/system.runtime.caching.memorycache?view=netframework-4.8
  - NCache - http://www.alachisoft.com/ncache/
  - redis - https://redis.io/documentation

**Usage 1: Get and Set cacheable object using single statement**

Try to get cacheable object by cache key. If the cacheable object is not in cache store, read data from data source and save the data in the cache store temporarily for certain time.
If the cached time expired, the cached object will be removed from the cache store. A list of dependency tags can be associated to the cacheable object. 
Application can invalidate cacheable object using dependency tags.
The Cache service choose a pluggable cache provider by the cache key using provider map key match.
A default cache provider will be selected if the cache key and provider map key doesn't match. 

```c#
    //Get and Set cacheable object
    var projectCategoryList = Cache.Get(
        "MyCache.1", //cache key name
        TimeSpan.FromMinutes(5), //Cache for 5 minutes
        new string[] {"ProjectCategory"}, //cache depenency tag.
        ()=>{ //retrieve cacheable data if data does not in cache store
            using(var db = new DbClient()){
                return db.Retrieve<ProjectCategory>(
                    (reader)=>{
                        return new ProjectCategory {
                            Code = reader.GetString("Category"),
                            Value = reader.GetString("Value")
                        }();
                        },
                        "select code, value from ProjectCategoryTable where orgid=:orgid",
                        DbParameters.New.Input("orgId",123)
                    );
                }
               }
            );
```

**Usage 2: Get and set cacheable object**


```c#
    //Get cacheable object. if the object is not cached, set cache data.
    
    List<ProjectCategory> projectCategoryList = Cache.Get(
        "MyCache.1", //cache key name
        TimeSpan.FromMinutes(5), //Cache for 5 minutes
        new string[] {"ProjectCategory"} //cache depenency tag.
        );

    if(projectCategory== null){
        //retrieve cacheable data if data does not in cache store
        using(var db = new DbClient()){
            projectCategory = db.Retrieve<ProjectCategory>(
                (reader)=>{
                    return new ProjectCategory {
                        Code = reader.GetString("Category"),
                        Value = reader.GetString("Value")
                    }();
                    },
                    "select code, value from ProjectCategoryTable where orgid=:orgid",
                    DbParameters.New.Input("orgId",123)
                );
               }
        if(projectCategory!=null){
            Cache.Set(
                "MyCache.1", //cache key name
                TimeSpan.FromMinutes(5), //Cache for 5 minutes
                new string[] {"ProjectCategory"}, //cache depenency tag.
                projectCategory
            );
        }
```

**Usage 3: Invalidate cacheable**

If the object has to be invalidated (For example, the object data state changed), the system will notify all the cache stores 
that this object is no longer up to date.

```c#
    //Invalidate cacheable object
    Cache.Invalidate("ProjectCategory");
```

**Usage 4: Remove cache object from cache store**


```c#
    Cache.Remove("MyCache.1");
```

**Usage 5: Choose a cache service explictly**



```c#
    //Get default Cache service
    ICache cache = new Cache();
    var cacheableObject = cache.Get("MyProject.1");
```

```c#
    //Get cache service by cache provider name
    ICache cache = new Cache("CacheProviderName");
```


## Cache Structure


<svg width="530" height="320" xmlns="http://www.w3.org/2000/svg">
 <!-- Created with Method Draw - http://github.com/duopixel/Method-Draw/ -->
 <g>
  <title>background</title>
  <rect fill="#fff" id="canvas_background" height="402" width="582" y="-1" x="-1"/>
  <g display="none" overflow="visible" y="0" x="0" height="100%" width="100%" id="canvasGrid">
   <rect fill="url(#gridpattern)" stroke-width="0" y="0" x="0" height="100%" width="100%"/>
  </g>
 </g>
 <g>
  <title>Layer 1</title>
  <ellipse ry="1" id="svg_7" cy="318.4375" cx="744.5" fill-opacity="null" stroke-opacity="null" stroke="#000" fill="none"/>
  <rect stroke="#000" id="svg_9" height="55" width="67" y="26.4375" x="14.5" fill-opacity="null" stroke-opacity="null" fill="none"/>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_10" cy="56.9375" cx="147.5" stroke-opacity="null" fill="#aaffff"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="24" id="svg_12" y="62.4375" x="26.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">App</text>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_13" y="60.4375" x="125.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object1</text>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_14" y2="53.4375" x2="121.5" y1="54.4375" x1="82.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <rect stroke="#000" id="svg_16" height="53" width="158" y="39.4375" x="229.5" stroke-opacity="null" stroke-width="null" fill="#aaffff"/>
  <text stroke="#000" transform="matrix(0.9477692971701431,0,0,1,6.950042713192275,0) " xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_17" y="61.4375" x="240.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" fill="#000000">Default Cache Provider</text>
  <rect stroke="#000" id="svg_18" height="39" width="87" y="36.4375" x="438.5" stroke-opacity="null" stroke-width="null" fill="#aaffff"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_19" y="60.4375" x="443.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Cache Store</text>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_20" y2="56.4375" x2="230.5" y1="56.4375" x1="172.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_22" y2="58.4375" x2="438.5" y1="57.4375" x1="386.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_23" cy="97.9375" cx="148.5" stroke-opacity="null" fill="#aaffff"/>
  <text style="cursor: move;" xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_24" y="103.4375" x="125.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object2</text>
  <path id="svg_27" d="m98.5,55.4375c0,0 -1,42 -1.5,41.5625c0.5,0.4375 27.5,0.4375 27,0" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <path id="svg_28" d="m175.5,96.4375c0,0 25,1 24.5,0.5625c0.5,0.4375 0.5,-40.5625 0,-41" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_34" y="79.4375" x="267.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Provider A</text>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_35" cy="145.9375" cx="148.5" stroke-opacity="null" fill="#aaffff"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_36" y="149.4375" x="125.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object3</text>
  <rect stroke="#000" id="svg_38" height="53" width="158" y="128.4375" x="230.5" stroke-opacity="null" stroke-width="null" fill="#aaffff"/>
  <text style="cursor: move;" stroke="#000" transform="matrix(0.9477692971701431,0,0,1,6.950042713192275,0) " xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_39" y="150.4375" x="241.55511" fill-opacity="null" stroke-opacity="null" stroke-width="0" fill="#000000">Named Cache Provider</text>
  <rect stroke="#000" id="svg_40" height="39" width="87" y="125.4375" x="439.5" stroke-opacity="null" stroke-width="null" fill="#aaffff"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_41" y="149.4375" x="444.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Cache Store</text>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_42" y2="145.4375" x2="231.5" y1="145.4375" x1="173.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_43" y2="147.4375" x2="439.5" y1="146.4375" x1="387.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_44" cy="188.9375" cx="149.5" stroke-opacity="null" fill="#aaffff"/>
  <text style="cursor: move;" xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_45" y="192.4375" x="126.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object4</text>
  <path id="svg_46" d="m99.5,144.4375c0,0 -1,42 -1.5,41.5625c0.5,0.4375 27.5,0.4375 27,0" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <path id="svg_47" d="m176.5,185.4375c0,0 25,1 24.5,0.5625c0.5,0.4375 0.5,-40.5625 0,-41" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_48" y="168.4375" x="268.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Provider A</text>
  <path id="svg_49" d="m92.5,51.4375c0,0 -1,95 -1,95c0,0 32,0 31.5,-0.4375" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_50" cy="237.9375" cx="148.5" stroke-opacity="null" fill="#aaffaa"/>
  <text style="cursor: move;" xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_51" y="241.4375" x="125.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object5</text>
  <rect stroke="#000" id="svg_52" height="53" width="158" y="220.4375" x="230.5" stroke-opacity="null" stroke-width="null" fill="#aaffaa"/>
  <text style="cursor: move;" stroke="#000" transform="matrix(0.9477692971701431,0,0,1,6.950042713192275,0) " xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_53" y="242.4375" x="241.55511" fill-opacity="null" stroke-opacity="null" stroke-width="0" fill="#000000">Named Cache Provider</text>
  <rect stroke="#000" id="svg_54" height="39" width="87" y="217.4375" x="439.5" stroke-opacity="null" stroke-width="null" fill="#aaffaa"/>
  <text xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_55" y="241.4375" x="444.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Cache Store</text>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_56" y2="237.4375" x2="231.5" y1="237.4375" x1="173.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <line stroke-linecap="null" stroke-linejoin="null" id="svg_57" y2="239.4375" x2="439.5" y1="238.4375" x1="387.5" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <ellipse stroke="#000" ry="17.5" rx="26" id="svg_58" cy="280.9375" cx="149.5" stroke-opacity="null" fill="#aaffaa"/>
  <text style="cursor: move;" xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_59" y="284.4375" x="126.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">object6</text>
  <path id="svg_60" d="m99.5,236.4375c0,0 -1,42 -1.5,41.5625c0.5,0.4375 27.5,0.4375 27,0" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <path id="svg_61" d="m176.5,277.4375c0,0 25,1 24.5,0.5625c0.5,0.4375 0.5,-40.5625 0,-41" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
  <text style="cursor: move;" xml:space="preserve" text-anchor="start" font-family="Helvetica, Arial, sans-serif" font-size="14" id="svg_62" y="260.4375" x="268.5" fill-opacity="null" stroke-opacity="null" stroke-width="0" stroke="#000" fill="#000000">Provider B</text>
  <path id="svg_63" d="m92.5,143.4375c0,0 -1,95 -1,95c0,0 32,0 31.5,-0.4375" fill-opacity="null" stroke-opacity="null" stroke-width="null" stroke="#000" fill="none"/>
 </g>
</svg>

SVG editor: https://editor.method.ac


## Configure pluggable Cache provider

The cache provider component can be plugged in the application through application configure file. 
It also can be added using code directly.

### Configure setting

Application Environment Configure setting

**plugin.config:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <!-- qshine environment config section -->
    <section name="qshine" type="qshine.Configuration.EnvironmentSection, qshine" />
  </configSections>

  <qshine>
    <!--plug-in component -->
    <components>
      <component name="ncache" interface="qshine.Caching.ICacheProvider" type="qshine.Caching.ncache.Provider, qshine.Caching.ncache"/>
      <component name="memorycache" interface="qshine.Caching.ICacheProvider" type="qshine.Caching.MemoryCacheProvider, qshine"/>
      <component name="redis" interface="qshine.Caching.ICacheProvider" type="qshine.Caching.RedisCacheProvider, qshine.Caching.redis"/>
    </components>

    <!--map cache framework system -->
    <maps name="qshine.Caching.ICacheProvider"  default="memorycache">
        <map key="MyCache" value="ncache" />
        <map key="OtherCache" value="redis" />
    </maps>

  </qshine>
</configuration>
```

