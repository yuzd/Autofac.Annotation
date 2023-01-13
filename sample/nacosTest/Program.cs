using Autofac;
using Autofac.Annotation;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 设置使用autofac作为DI容器
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());


// 设置使用nacos动态配置
builder.Host.UseNacosConfig("NacosConfig", parser: null, logAction: null);

// builder.Host.ConfigureAppConfiguration((c, cfb) =>
// {
//     IConfigurationRoot configurationRoot = cfb.Build();
//     var sourceBuilders = cfb.AddNacosV2Configuration(configurationRoot.GetSection("NacosConfig"), parser: null, logAction: null);
//     var nacosProvider = sourceBuilders.Sources.LastOrDefault(r => r.GetType() == typeof(NacosV2ConfigurationSource));
//     c.Properties["nacosProvider"] = nacosProvider;
// });

// 设置DI容器 添加注解功能
builder.Host.ConfigureContainer<ContainerBuilder>((c, containerBuilder) =>
{
    // 注册nacos的source到DI容器中去
    // if (c.Properties["nacosProvider"] is IConfigurationSource nacosSource)
    // {
    //     var configurationProvider = nacosSource.Build(null);
    //     containerBuilder.RegisterInstance(configurationProvider).Keyed<IConfigurationProvider>("nacos").SingleInstance();
    // }

    containerBuilder.RegisterModule(new AutofacAnnotationModule()
            .SetDefaultValueResource(c.Configuration) //替换成包含有nacos动态源的
    );
});

// 设置Controller实例的生成也交给autofac
builder.Host.ConfigureServices(services => { services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>()); });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();