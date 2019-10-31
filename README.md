
# Developing a Microsoft .NET Core Web API application with Aurora Database using CloudFormation. 

**As part of this blog we will do the following.**

1. Use/convert existing Microsoft .NET Core WebAPI's and integrate AWS SDK Package like SSM into it.

2. Utilize Elastic Container Service (ECS), Elastic Container Registry (ECR), Amazon Systems Manager (SSM) [maintains the Aurora DB Credentials]

3. Cloudformation to spin up the AWS Infrastructure and Services required for this application to run.

4. CodeCommit, CodeBuild tools for development/build CI/CD pipeline setup.

4. Amazon Aurora (Serverless) Database for better database freedom.

### Design Considerations

1. ECS in public subnet. RDS in private subnet. Other combinations of private subnet also exist

2. Store database credentials in SSM. Used in second stack to spin up the TODOS - table before ECS service spins up

3. TODOS table can be created as part of Dotnet application also. Other possible design options also exist.

4. LoadBalancer is created without HTTPS/TLS. Real world this should not be the case 

### Steps

1. Execute the below command to spin up the infrastructure cloudformation stack (parent stack)

   ```
   aws cloudformation create-stack --stack-name net-core-stack --template-body file://config/net-core-task-infrastucture.yaml  --capabilities CAPABILITY_NAMED_IAM --parameters  ParameterKey=AppStackName,ParameterValue=net-core-stack
   
   ```

2. Check in the .NET Core WebAPI code (sample code provided) to AWS CodeCommit. This will automatically trigger CodeBuild which compiles the code pushes the docker image to  Amazon ECR.

3. Execute the below command to create the ECS Service stack. Service has the task definition to read the image from ECR. This command exposes the HealthCheckURL and the WebAPIURL (that interacts with TODO database) as Outputs in the CloudFormation.

**Note:** The below command takes StackName, KeyName (EC2 key pair YOUR account that is already created). The "StackName" parameter is provided as a parameter in the above stack. It uses the parent (first CloudFormation) ex: net-core-stack by default in the yaml. You can also provide that as a parameter if you have a different name for your first stack

   ```

   aws cloudformation create-stack --stack-name net-core-service-stack --template-body file://config/net-core-task-services.yaml  --capabilities CAPABILITY_NAMED_IAM --parameters  ParameterKey=AppStackName,ParameterValue=net-core-service-stack ParameterKey=KeyName,ParameterValue=my-east1-keypair ParameterKey=StackName,ParameterValue=net-core-stack

   ```

4. The WebAPI is exposed to the outside world using Public LoadBalancer.

### Test

1. From the output of the second stack use the "WebApiUrl" to test the api.

2. You can use tools like Postman, ARC Rest Client or Browser extensions like RestMan

3. Select "content-type" as "application/json"

4. POST as rawdata/json - sample below

   ```
   {
      "Task": "new TODO Application",
      "Status": "Completed"
   }
   ```
5. Use the same url and fire a GET call to see the previously posted todo item as response.


### Clean Up

**Make sure to check the following are deleted before the delete stacks are performed**

   - Contents of the S3 files are deleted
      - S3 bucket will in the format <stack-name>-region-accountId. ex: net-core-stack-us-east-1-1234567890

   - ECR (Container Registry) repository is deleted
      - Container repository will be in the format <stack-name>-todo-repository. ex: net-core-stack-todo-repository

   - EC2 RDS Table Creator instance is terminated 
      - The name will be in the format: <service-stack-name>--rds-table-creator-instance. ex: net-core-service-stack-rds-table-creator-instance

**Once above steps are completed. Execute the below commands:**
- $ aws cloudformation delete-stack --stack-name <services-stack-name>
   - Eg: $ aws cloudformation delete-stack --stack-name net-core-service-stack
- $ aws cloudformation delete-stack --stack-name <stack-name>
   - Eg: $ aws cloudformation delete-stack --stack-name net-core-stack


### Troubleshooting

1) Make sure to review the AWS service limits. Ex: 5 VPCs per region

2) After the service stack completes if the WebAPIURL displays blank
   - In AWS Console, navigate to EC2 section and make sure the <stack-name>-tablecreatorinstance is running 2/2 complete
      - This EC2 creates the "ToDos" table in the "todo" database in Aurora
   - If issue persist, Navigate to RDS in AWS Console
      - Validate if the "ToDos" table is created in the Aurora database
         - select your database typically <stack-name>-todo
         - click "Modify". Scroll down and select "Data API", Apply immediately in next screen - This lets you to query the DB using AWS RDS Console query editor
         - select "Query Editor" - Provide the connection string. Typically this is available in the first/infrastructure stack
         - in the Query window - execute below SQLs to validate
            ```

            use todo;
            select * from ToDos;

            ```
            If this doesn't return records you can manually create the table with below query
            
            ```
            use todo;
            drop table IF EXISTS ToDos; 
            create table ToDos(
               id MEDIUMINT not null auto_increment,
               CreatedTime TIMESTAMP DEFAULT now(),
               Status VARCHAR(50),
               Task VARCHAR(50),
               primary key(id)
            );
            select * from ToDos;

            '''

      ##### Note: Inactivity in Aurora Serverless - RDS Table could put the RDS in suspended state to reduce the cost. You might receive a communication error after no activity while trying to invoke the database DDL/DML statements. You can notice this by connecting to the SQL in Query Editor with below output.
         - Communications link failure The last packet sent successfully to the server was 0 milliseconds ago. The driver has not received any packets from the server.
      Retrying the select queries will warm up the RDS database for subsequent connection to be served.      

## License

This library is licensed under the MIT-0 License. See the LICENSE file.



