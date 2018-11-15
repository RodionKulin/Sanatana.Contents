# Description and benefits
-Sanatana.Contents provides content CRUD operations, global search, attachments and comments support, html sanitizing and permissions control.

-Create, Update, Delete operations based on Sanatana.Patterns.Pipelines project that allows reusing and altering predefined pipelines to custom needs.

-It is a backend implementation with no built in UI. It can be wired to existing solution requiring content management.

-NET standard 2.0 version, so it can be used in both NET environments: Full, Core.

-Basic role based permission system lets you control access to different content types. No built in default roles and permission types means you can define em on your application level as enum and supply as integers to Sanatana.Contents.

-Everything is extendable and replaceable through dependency injection. Autofac as DI container is recommended, although not restricted to it.

-Technologies used allow to serve a small load single instance or high load distributed content application. EntityFrameworkCore and MongoDb are provided as data access layers. ElasticSearch as search engine. Amazon S3 or OS file system as files storage.  

-Maximum simplicity of adding new content types. Edit DbContext to add mapping for new entity and inherit it from base Content type. To introduce custom entities to ElasticSearch Automapper configuration is required on top of usual ElasticSearch mapping.

-Have multiple content types in single application. Like posts, support tickets, shop items, you name it. Entities that have free input text as some of their properties are likely to fit.
