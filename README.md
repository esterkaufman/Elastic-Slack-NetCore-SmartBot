# ResearcherBot

# Services Diagram


  
```                                 
|----------------------------------| 
|  Researcher.Bot.Service          |  
|----------------------------------|  
  |		    |              |
  |		    |              |
  |		    |              |
  |		    |              |
  |		    |              |
  |		    |           |---------------------------------| 
  |		    |           |  Researcher.Bot.Implementations |
  |		    |           |---------------------------------|
  |	|----------------------------------|         |      |
  |	| Researcher.Bot.Integration.Slack |         |      |
  |	|----------------------------------|         |      |
|------------------------------------------|     |      |
| Researcher.Bot.Integration.ElasticSearch |     |      |
|------------------------------------------|     |      |
                                             |-------| |---------------|
                                             | Slack | | ElasticSearch |
                                             |-------| |---------------|
```																					 
	
	
## Researcher.Bot.Service:
	Technology: .NetCore 3.1
	Public to world, handling requests
	Calls to AddSlackIntegration
	Calls to AddElasticSearchIntegration
	Calls to implementations from Researcher.Bot.Implementations -> Slack, ElasticSearch
	
	

## Researcher.Bot.Implementations:
	Pure c#
    ResearcherBot implementation for any integrations
	(Currently Available: Slack, ElasticSearch)

	

## Researcher.Bot.Integration.Slack:
	Pure c#								
    Generic & Reusable 					
    Integrations with Slack Api library

	
## Researcher.Bot.Integration.ElasticSearch:
	Pure c#								
    Generic & Reusable 					
    Integrations with ElasticSearch & Kibana Api	
