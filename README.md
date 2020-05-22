![build and test](https://github.com/thebeebs/LambdaChaosInjection/workflows/build%20and%20test/badge.svg)

# Chaos Injection for AWS Lambda - ChaosLambdaInjection

ChaosLambdaInjection is a small library injecting chaos into AWS Lambda. 
It offers simple decorators to do delay, exception and statusCode injection 
and some methods to add delay to any 3rd party dependencies called from your function. 
This allows to conduct small chaos engineering experiments 
for your serverless application in the AWS Cloud.

- Support for Latency injection using delay
- Support for Exception injection using exception_msg
- Support for HTTP Error status code injection using error_code
- Using for SSM Parameter Store to control the experiment using isEnabled
- Support for adding rate of failure using rate. (Default rate = 1)
- Per Lambda function injection control using Environment variable (CHAOS_PARAM)

## Install
```bash
dotnet add package LambdaChaosInjection --version 0.1.1
```

## Example
We can add a decorator to a function to inject a chaos policy
```csharp
[InjectDelayPolicy]
public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
{
    var location = await GetCallingIP();
    var body = new Dictionary<string, string>
    {
        {"message", "hello world"},
        {"location", location}
    };

    return new APIGatewayProxyResponse
    {
        Body = JsonConvert.SerializeObject(body),
        StatusCode = 200,
        Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
    };
}
```
Alternatively you can wrap a portion of code in a ChaosWrap. 
This is ultimately what the decorator is doing anyway and I suspect 
that is slightly better in terms of performance.   

```csharp
public async Task<APIGatewayProxyResponse> FunctionHandlerException(APIGatewayProxyRequest apigProxyEvent, ILambdaContext context)
{
    return await new ChaosWrap<InjectException>().Execute(async () =>
        {
            var location = await GetCallingIP();
            var body = new Dictionary<string, string>
            {
                {"message", "hello world"},
                {"location", location}
            };

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(body),
                StatusCode = 200,
                Headers = new Dictionary<string, string> {{"Content-Type", "application/json"}}
            };
        }
    );
}
```

## Configuration
The configuration for the failure injection is stored in the [AWS SSM Parameter Store](https://aws.amazon.com/ssm/):
```
{
    "isEnabled": true,
    "delay": 400,
    "error_code": 404,
    "exception_msg": "I really failed seriously",
    "rate": 1
}
```
To store the above configuration into SSM using the AWS CLI do the following:

```bash
aws ssm put-parameter --region eu-north-1 --name chaoslambda.config --type String --overwrite --value "{ "delay": 400, "isEnabled": true, "error_code": 404, "exception_msg": "I really failed seriously", "rate": 1 }"
```

AWS Lambda will need to have [IAM access to SSM](https://docs.aws.amazon.com/systems-manager/latest/userguide/sysman-paramstore-access.html).
```
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "ssm:DescribeParameters"
            ],
            "Resource": "*"
        },
        {
            "Effect": "Allow",
            "Action": [
                "ssm:GetParameters",
                "ssm:GetParameter"
            ],
            "Resource": "arn:aws:ssm:eu-north-1:12345678910:parameter/chaoslambda.config"
        }
    ]
}
```
## Supported Decorators:
   ChaosLambdaInjection currently supports the following decorators:
   
   [InjectDelay] - add delay in the AWS Lambda execution
   [InjectException] - Raise an exception during the AWS Lambda execution
   [InjectStatusCode] - force AWS Lambda to return a specific HTTP error code

## Project Files

This project contains source code and supporting files for the LamdaChaosInjection Nuget package.

## Unit tests

Tests are defined in the `Tests` folder in this project.

```bash
dotnet test Tests
```

- Samples - Code for two sample Lambda functions that use Chaos Injection.
- test - Unit tests for the LambdaChaosInjection project. 
- template.yaml - A template that defines the application's AWS resources should you wish to deploy the sample functions to your AWS account.

The application uses several AWS resources, including Lambda functions and an API Gateway API. These resources are defined in the `template.yaml` file in this project. You can update the template to add AWS resources through the same deployment process that updates your application code.

If you prefer to use an integrated development environment (IDE) to build and test your application, you can use the AWS Toolkit.  
The AWS Toolkit is an open source plug-in for popular IDEs that uses the SAM CLI to build and deploy serverless applications on AWS. The AWS Toolkit also adds a simplified step-through debugging experience for Lambda function code. See the following links to get started.

* [Rider](https://docs.aws.amazon.com/toolkit-for-jetbrains/latest/userguide/welcome.html)
* [VS Code](https://docs.aws.amazon.com/toolkit-for-vscode/latest/userguide/welcome.html)
* [Visual Studio](https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/welcome.html)

## Deploy the sample application

The Serverless Application Model Command Line Interface (SAM CLI) is an extension of the AWS CLI that adds functionality for building and testing Lambda applications. It uses Docker to run your functions in an Amazon Linux environment that matches Lambda. It can also emulate your application's build environment and API.

To use the SAM CLI, you need the following tools.

* SAM CLI - [Install the SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-install.html)
* .NET Core - [Install .NET Core](https://www.microsoft.com/net/download)
* Docker - [Install Docker community edition](https://hub.docker.com/search/?type=edition&offering=community)

To build and deploy your application for the first time, run the following in your shell:

```bash
sam build
sam deploy --guided
```

The first command will build the source of your application. The second command will package and deploy your application to AWS, with a series of prompts:

* **Stack Name**: The name of the stack to deploy to CloudFormation. This should be unique to your account and region, and a good starting point would be something matching your project name.
* **AWS Region**: The AWS region you want to deploy your app to.
* **Confirm changes before deploy**: If set to yes, any change sets will be shown to you before execution for manual review. If set to no, the AWS SAM CLI will automatically deploy application changes.
* **Allow SAM CLI IAM role creation**: Many AWS SAM templates, including this example, create AWS IAM roles required for the AWS Lambda function(s) included to access AWS services. By default, these are scoped down to minimum required permissions. To deploy an AWS CloudFormation stack which creates or modified IAM roles, the `CAPABILITY_IAM` value for `capabilities` must be provided. If permission isn't provided through this prompt, to deploy this example you must explicitly pass `--capabilities CAPABILITY_IAM` to the `sam deploy` command.
* **Save arguments to samconfig.toml**: If set to yes, your choices will be saved to a configuration file inside the project, so that in the future you can just re-run `sam deploy` without parameters to deploy changes to your application.

You can find your API Gateway Endpoint URL in the output values displayed after deployment.

## Use the SAM CLI to build and test locally

Build your application with the `sam build` command.

```bash
sam build
```

The SAM CLI installs dependencies defined in `src/HelloWorld.csproj`, creates a deployment package, and saves it in the `.aws-sam/build` folder.

Test a single function by invoking it directly with a test event. An event is a JSON document that represents the input that the function receives from the event source. Test events are included in the `events` folder in this project.

Run functions locally and invoke them with the `sam local invoke` command.

```bash
sam local invoke HelloWorldFunction --event events/event.json
```

The SAM CLI can also emulate your application's API. Use the `sam local start-api` to run the API locally on port 3000.

```bash
sam local start-api
curl http://localhost:3000/
```

The SAM CLI reads the application template to determine the API's routes and the functions that they invoke. The `Events` property on each function's definition includes the route and method for each path.

```yaml
      Events:
        HelloWorld:
          Type: Api
          Properties:
            Path: /hello
            Method: get
```

## Add a resource to your application
The application template uses AWS Serverless Application Model (AWS SAM) to define application resources. AWS SAM is an extension of AWS CloudFormation with a simpler syntax for configuring common serverless application resources such as functions, triggers, and APIs. For resources not included in [the SAM specification](https://github.com/awslabs/serverless-application-model/blob/master/versions/2016-10-31.md), you can use standard [AWS CloudFormation](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/aws-template-resource-type-ref.html) resource types.

## Fetch, tail, and filter Lambda function logs

To simplify troubleshooting, SAM CLI has a command called `sam logs`. `sam logs` lets you fetch logs generated by your deployed Lambda function from the command line. In addition to printing the logs on the terminal, this command has several nifty features to help you quickly find the bug.

`NOTE`: This command works for all AWS Lambda functions; not just the ones you deploy using SAM.

```bash
sam logs -n HelloWorldFunction --stack-name AWS --tail
```

You can find more information and examples about filtering Lambda function logs in the [SAM CLI Documentation](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/serverless-sam-cli-logging.html).


## Cleanup

To delete the sample application that you created, use the AWS CLI. Assuming you used your project name for the stack name, you can run the following:

```bash
aws cloudformation delete-stack --stack-name AWS
```

## Resources

See the [AWS SAM developer guide](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/what-is-sam.html) for an introduction to SAM specification, the SAM CLI, and serverless application concepts.

