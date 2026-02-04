using Brightcove.DataExchangeFramework.Helpers;
using Sitecore.DataExchange.ApplyMapping;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.DataAccess;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Models;
using Sitecore.Services.Core.Diagnostics;
using System;
using System.Linq;
using MappingSettings = Sitecore.DataExchange.ApplyMapping.MappingSettings;

namespace Brightcove.DataExchangeFramework.Processors
{
    public class ApplyMappingPipelineStepProcessor : ApplyMappingStepProcessor
    {
        protected override void ProcessPipelineStep(PipelineStep pipelineStep = null, PipelineContext pipelineContext = null, ILogger logger = null)
        {
            try
            {
                MappingSettings mappingSettings = pipelineStep.GetMappingSettings();
                if (mappingSettings == null)
                {
                    Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because the pipeline step is missing a plugin.", new string[1]
                    {
                        "plugin: " + typeof (MappingSettings).FullName
                    });
                }
                else
                {
                    IMappingSet mappingSet = mappingSettings.MappingSet;
                    if (mappingSet == null)
                        Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because the pipeline step has no mapping set assigned.", new string[2]
                        {
                            "plugin: " + typeof (MappingSettings).FullName,
                            "property: MappingSet"
                        });
                    else if (mappingSet.Mappings == null)
                    {
                        Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because the pipeline step has no mappings assigned.", new string[2]
                        {
                            "plugin: " + typeof (MappingSettings).FullName,
                            "property: MappingSet"
                        });
                    }
                    else
                    {
                        object sourceObject = GetSourceObject(mappingSettings, pipelineContext, logger);
                        if (sourceObject == null)
                        {
                            Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because no source object could be resolved from the pipeline context.", new string[1]
                            {
                                string.Format("source location: {0}", mappingSettings.SourceObjectLocation)
                            });
                        }
                        else
                        {
                            object targetObject = GetTargetObject(mappingSettings, pipelineContext, logger);
                            if (targetObject == null)
                            {
                                Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because no target object could be resolved from the pipeline context.", new string[1]
                                {
                                    string.Format("target location: {0}", mappingSettings.TargetObjectLocation)
                                });
                            }
                            else
                            {
                                MappingContext mappingContext = new MappingContext()
                                {
                                    Source = sourceObject,
                                    Target = targetObject
                                };
                                if (!mappingSet.Run(mappingContext))
                                {
                                    Log(new Action<string>(logger.Error), pipelineContext, "Pipeline step processing will abort because mapping set failed.", new string[3]
                                    {
                                        string.Format("mappings that succeeded: {0}", mappingContext.RunSuccess.Count),
                                        string.Format("mappings that were not attempted: {0}", mappingContext.RunIgnore.Count),
                                        string.Format("mappings that failed: {0}", mappingContext.RunFail.Count)
                                    });
                                    Log(new Action<string>(logger.Error), pipelineContext, "At least one required value mapping failed.", new string[2]
                                    {
                                        string.Format("mappings that failed: {0}", mappingContext.RunFail.Count),
                                        "value mapping ids: " + string.Join(",", mappingContext.RunFail.Select(x => x.Identifier).ToArray())
                                    });

                                    HandleError(pipelineContext);
                                }
                                else
                                {
                                    if (mappingContext.RunFail.Any())
                                    {
                                        Log(new Action<string>(logger.Error), pipelineContext, "At least one value mapping failed.", new string[2]
                                        {
                                            string.Format("mappings that failed: {0}", mappingContext.RunFail.Count),
                                            "value mapping ids: " + string.Join(",", mappingContext.RunFail.Select(x => x.Identifier).ToArray())
                                        });

                                        HandleError(pipelineContext);
                                    }

                                    pipelineContext.GetSynchronizationSettings().IsTargetDirty = IsTargetDirty(mappingContext, mappingSettings, pipelineContext, logger);
                                    if (!ShouldRunMappingsAppliedActions(mappingContext, mappingSettings, pipelineContext, logger))
                                        return;
                                    RunMappingsAppliedActions(mappingContext, mappingSettings, pipelineContext, logger);
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                logger.Error("Failed to apply mapping(s) because an unexpected error occured", ex);
                HandleError(pipelineContext);
            }
        }

        protected void HandleError(PipelineContext pipelineContext)
        {
            BrightcoveSyncSettingsHelper.SetErrorFlag(pipelineContext);
            pipelineContext.Finished = true;
            pipelineContext.CriticalError = false;
        }
    }
}
