using __MODULE_NAME__.Api;
using __MODULE_NAME__.Contracts.Schema;
using __MODULE_NAME__.Infrastructure.Data;
using NUnit.Framework;

namespace __MODULE_NAME__.Tests;

public sealed class __MODULE_NAME__ModuleScaffoldTests
{
    [Test]
    public void Module_name_and_schema_follow_conventions()
    {
        var module = new __MODULE_NAME__Module();

        Assert.Multiple(() =>
        {
            Assert.That(module.Name, Is.EqualTo("__MODULE_NAME__"));
            Assert.That(__MODULE_NAME__Schema.Name, Is.EqualTo("__MODULE_SCHEMA__"));
            Assert.That(__MODULE_NAME__DbContext.Schema, Is.EqualTo(__MODULE_NAME__Schema.Name));
        });
    }
}
