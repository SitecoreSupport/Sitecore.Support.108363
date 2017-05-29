using System;
using System.Linq;
using Sitecore.Analytics.Data.Items;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Models;
using Sitecore.ContentTesting.Pipelines.AddPersonalizationVariables;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Layouts;
using Sitecore.Rules;
using Sitecore.Rules.ConditionalRenderings;

namespace Sitecore.Support.ContentTesting.Pipelines.AddPersonalizationVariables
{
    public class AddRules : AddPersonalizationVariablesProcessor
    {
        public override void Process(AddPersonalizationVariablesPipelineArgs args)
        {
            Item item = args.Item;
            //added check on Device.ID is null
            if (item == null || args.DeviceId == (ID)null)
            {
                return;
            }
            DeviceItem deviceItem = item.Database.Resources.Devices[args.DeviceId];
            if (deviceItem == null)
            {
                return;
            }
            RenderingReference[] personalizationRenderings = ContentTestingFactory.Instance.PersonalizationTestStore.LoadConfiguration(item, deviceItem).GetPersonalizationRenderings(false);
            if (!personalizationRenderings.Any<RenderingReference>())
            {
                return;
            }
            RenderingReference[] array = personalizationRenderings;
            for (int i = 0; i < array.Length; i++)
            {
                RenderingReference renderingReference = array[i];
                if (!string.IsNullOrEmpty(renderingReference.Settings.PersonalizationTest))
                {
                    Guid a;
                    Guid.TryParse(renderingReference.Settings.PersonalizationTest, out a);
                    if (!(a == Guid.Empty) && !(a != args.Builder.Instance.Id))
                    {
                        string webEditDisplayName = renderingReference.WebEditDisplayName;
                        TestVariable.Builder builder = args.Builder.AddVariable(Guid.Parse(renderingReference.UniqueId), webEditDisplayName, PersonalizationTestVariableItem.TemplateID.ToGuid(), new Guid?(ItemIDs.Analytics.DefaultCondition.ToGuid()));
                        foreach (Rule<ConditionalRenderingsRuleContext> current in renderingReference.Settings.Rules.Rules)
                        {
                            if (!args.FilteredVariations.ContainsKey(renderingReference.UniqueId) || !args.FilteredVariations[renderingReference.UniqueId].Contains(current.UniqueId))
                            {
                                Guid valueId = current.UniqueId.ToGuid();
                                builder.AddVariation(valueId, current.Name, valueId.Equals(ItemIDs.Analytics.DefaultCondition.ToGuid()));
                            }
                        }
                    }
                }
            }
        }
    }
}