<h1 align="center">HttpClient78</h1>
<div align="center">


「HttpClient78」封装HttpClient，方便调用。

[![License](https://img.shields.io/badge/license-Apache%202-green.svg)](https://www.apache.org/licenses/LICENSE-2.0)
[![Build Status](https://dev.azure.com/www778878net/basic_csharp/_apis/build/status/www778878net.HttpClient78?branchName=main)](https://dev.azure.com/www778878net/basic_csharp/_build/latest?definitionId=19&branchName=main)
[![QQ群](https://img.shields.io/badge/QQ群-323397913-blue.svg?style=flat-square&color=12b7f5&logo=qq)](https://qm.qq.com/cgi-bin/qm/qr?k=it9gUUVdBEDWiTOH21NsoRHAbE9IAzAO&jump_from=webapi&authKey=KQwSXEPwpAlzAFvanFURm0Foec9G9Dak0DmThWCexhqUFbWzlGjAFC7t0jrjdKdL)

</div>

## API文档地址：[http://www.778878.net/docs/](http://www.778878.net/docs/#/HttpClient78/)
## 反馈qq群(点击加群)：[323397913](https://qm.qq.com/cgi-bin/qm/qr?k=it9gUUVdBEDWiTOH21NsoRHAbE9IAzAO&jump_from=webapi&authKey=KQwSXEPwpAlzAFvanFURm0Foec9G9Dak0DmThWCexhqUFbWzlGjAFC7t0jrjdKdL)

## 简介 introduction

1. 简单封装HttpClient 减少学习成本
2. 支持数种不同的返回值



## 适用端 apply

**use for `.net6.0` project**



## 安装 rely on

nuget 安装 HttpClient78

## 属性 props

详见API文档地址

## 方法 method

详见API文档地址

## DEMO 

```c#
//更多示例 详见文档链接
using www778878net.Net;

public async void GetToStringTest()
{
	Uri uri = new("http://net.778878.net/apinet/services/Services78/test");
	var tmp = await HttpClient78.Client78.GetToString(uri);
	string? getback = tmp!.Content;
 
	 
}

```

## OTHER
https://github.com/www778878net/HttpClient78.git
you can see Test78/
