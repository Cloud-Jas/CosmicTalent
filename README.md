## About

CosmicTalent, an application designed to empower HR and managers in effectively navigating employee information and efficiently filtering or identifying eligible employees based on specific task requirements.

## Inspiration

Currently in Service-based companies, it is highly challenging to find and retain the top talents to meet the needs of their clients. Traditional method of choosing a talent for client requirement is not efficient, as it is tend to be lengthy process and would have mismatches between the candidate and roles. Also it is difficult to understand the expectations of the talent being hired.

However with our <b>CosmicTalent</b> app, now companies could make use or Vector search capabilities to better retrieve employees for a specific requirement and also know about their employees better. This will help onboarding the best-fit candidates for the client's requirement and also improve retainment of talents by understanding their unique capabilities more efficiently.

<b> PS: Another flavor of this app could be used for Talent selection . Gone are the days of manual resume screening. Now with our app we have option to onboard best-fit talents </b>

## Architecture

<a class="lightgallery" href="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/dj7lfum2k7vnd2kju9bc.gif" title="Image description" data-thumbnail="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/dj7lfum2k7vnd2kju9bc.gif">
 <img class="lazyautosizes lazyloaded" data-src="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/dj7lfum2k7vnd2kju9bc.gif" src="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/dj7lfum2k7vnd2kju9bc.gif" width="1700">
 </a>

## Data Flow

### Document Intelligence Process

- Initially, we gather a collection of resume documents from our employees, all adhering to a similar template.
- Using Azure Document Intelligence, we employ a Custom Extraction Model to construct a tailored schema for our employee resumes, annotating and labeling relevant sections.
- This model is then trained and refined to create a custom extraction model, which will be then used in subsequent stages of our application.

###  Resume Upload Process

- Resumes are uploaded by HR/Managers onto our platform, where they are directed to an Blob Processor (Azure function), within a container app environment, for processing.
- The function stores the file in blob storage and retrieves the specific schema with their relevant informations, utilizing the custom extraction model generated in the Document Intelligence process.
- Subsequently, this schema along with relevant information is transmitted to an Azure Service Bus message broker.
- A Worker Service (Azure Container App), within a container app environment , then picks up the schema for processing.
- The Worker Service segments the documents into chunks based on various field combinations, ensuring each chunk contains essential information like Employee ID and Name to facilitate efficient retrieval. Summarization is applied only for Project details and skipped for other field combinations.
- The Azure OpenAI embedding model is employed to generate vectors from these chunks, which are subsequently stored in separate collections within Azure CosmosDB Mongo vCore.

### Copilot Process

- When HR/Managers make queries or requests within our chat app, these are forwarded to the Talent Processor, another containerized application.
- Talent Processor translate the query or prompt into vector embeddings.
- These embeddings are then utilized in a vector search against our database to identify semantically matching records.
- Retrieved documents are then passed to the GPT-3.5 Turbo model to generate appropriate responses.
- Finally, these responses are relayed back to our chat app for interaction with HR/Managers.

## Refinement of RAG flow

<a class="lightgallery" href="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3hzhwur5pzidwiokbzgj.png" title="Image description" data-thumbnail="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3hzhwur5pzidwiokbzgj.png">
 <img class="lazyautosizes lazyloaded" data-src="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3hzhwur5pzidwiokbzgj.png" src="https://dev-to-uploads.s3.amazonaws.com/uploads/articles/3hzhwur5pzidwiokbzgj.png" width="1700">
 </a>

- The JSON response from the Custom Extraction model encompasses all pertinent details of the resume document.
- Subsequently, it undergoes segmentation into individual chunks, each prefaced with essential information regarding Employee ID and Employee Name to facilitate efficient retrieval.
- Chunks pertinent to projects are directed to the summarizer before proceeding to the embedding model for vector embedding generation.
- Meanwhile, other chunks bypass the summarizer and are directly forwarded to the embedding model for vector embedding generation.
- Finally, these embeddings are stored in Azure CosmosDB Mongo vCore.

### GetResumeEmbeddingChunks Code

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/rt9oyusuqazqdy232c6y.png)

- In the code snippet above, the function GetResumeEmbeddingChunks processes a list of document fileds retrieved from Azure Document Intelligence using our Custom Extraction model.
- We employ the Task Parallel Library (TPL) to iterate through each field or combination of fields, refining the chunking process. This approach has proven to be more effective than traditional chunking strategies, attached results in the next section.

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/4upqmeujtrzte163vfmz.png)

- As the chunks lose information about EmployeeId and EmployeeName, we preload this context across all the chunks.
- The chunks are then sent to Azure OpenAI to generate vector embeddings using the text-embedding-ada model.

### Custom Summarization Logic

- While there are various summarization patterns available for long documents, they may not be effective for our specific use case.
- Therefore, in the code snippet above, only fields related to projects are further summarized using Azure OpenAI before generating vector embeddings.
- This refinement has led to improved token consumption and better retrieval of relevant information.

## Better results after Refinement of RAG flow 

The initial version of our application did not consider any chunking patterns and generated vector embeddings for the entire content within the Employee collection. Here were the issues encountered before refining the RAG flow:

 ## Challenges before RAG Flow Refinement

 <b> 1. Not able to retrieve information effectively</b>

 ![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/awxuzuv1h7ibkye678eq.png)

 <b> 2. Token exceed limitations </b>

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/rypzing1foi3js0xx7t8.png)


## Results After RAG Flow Refinement

<b> 1. Able to retrieve information effectively </b>

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/16xa3f1k6hv53qfs3x5n.png)

<b> 2. Improvements in overall Token consumption </b>

![Image description](https://dev-to-uploads.s3.amazonaws.com/uploads/articles/ec5d61sgs1awzeujyzhb.png)
