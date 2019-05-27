### Basic functionality
-Sanatana.Contents library provides content, attached images, comments and categories management;<br/>
-Create, Update, Delete operations based on Sanatana.Patterns.Pipelines library allow reusing and altering predefined sequences of actions. Validation, html sanitizing, permission checks, database access and other steps can be removed or replaced with domain specific;<br/>
-Basic role permission system lets control access of users to different content types. Custom roles, permissions and content statuses are supported. Define them as domain specific enums and supply as integers to Sanatana.Contents;<br/>
-LINQ to ElasticSearch support for global search queries with easy filtering settings.<br/>

### Widely applicable
-It is a backend set of libraries with no built in UI. It can be wired to existing solution requiring content management;<br/>
-NET standard 2.0 version, so it can be used in both .NET frameworks: Full, Core;<br/>
-Async await used through all layers.

### Scalable
-Supporting libraries to choose from allow serving content in high volume distributed applications:<br/>
EntityFrameworkCore or MongoDb as data access layers;<br/>
ElasticSearch as search engine;<br/>
Amazon S3 or OS file system as files storage;<br/>
Configurable caching layer decorating database queries.

### Extensible
-Everything is extendable and replaceable through dependency injection. Autofac modules are provided out of the box, although not restricted to using them. Any other DI container can be used;<br/>
-Possible to have multiple content types in single application. Like posts, support tickets, shop items or something else. Each saved in separate database table. Entities that have free input text as some of their properties are likely candidates;<br/>
-Simple way of adding custom content types, not much different from applications from scratch. While still reusing all existing business logic with help of generics.

### Documentation
[Wiki](https://github.com/RodionKulin/Sanatana.Contents/wiki) pages describing:<br/>
-Configuration<br/>
-Usage<br/>
-Extensibility