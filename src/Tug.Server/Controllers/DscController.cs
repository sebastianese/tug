/*
 * Copyright © The DevOps Collective, Inc. All rights reserved.
 * Licnesed under GNU GPL v3. See top-level LICENSE.txt for more details.
 */

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Tug.Messages;
using Tug.Model;

namespace Tug.Server.Controllers
{
    /// <summary>
    /// A controller that implements the core v2 requests for a
    /// DSC Pull Server, including registration, status checking,
    /// configuration retrieval and module retrieval. 
    /// </summary>
    public class DscController : Controller
    {
        private ILogger<DscController> _logger;
        private IDscHandlerProvider _dscHandlerProvider;
        public DscController(ILogger<DscController> logger,
                IDscHandlerProvider handlerProvider)
        {
            _logger = logger;
            _dscHandlerProvider = handlerProvider;
        }

        [HttpPut]
        [Route("Nodes(AgentId='{AgentId}')")]
        //TODO:  [Authorize]
        public IActionResult RegisterDscAgent(RegisterDscAgentRequest input)
        {
            _logger.LogInformation("\n\n\nPUT: Node registration");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    h.RegisterDscAgent(input.AgentId, input.Body);
                }

                return this.Model(RegisterDscAgentResponse.INSTANCE);
            }

            return base.BadRequest(ModelState);
        }

        [HttpPost]
        [Route("Nodes(AgentId='{AgentId}')/GetDscAction")]
        public IActionResult GetDscAction(GetDscActionRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: DSC action request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}]");

                ActionStatus actionInfo;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    actionInfo = h.GetDscAction(input.AgentId, input.Body);
                }

                var response = new GetDscActionResponse
                {
                    Body = new GetDscActionResponseBody
                    {
                        NodeStatus = actionInfo.NodeStatus,
                        Details = actionInfo.ConfigurationStatuses?.ToArray(),
                    }
                };

                return this.Model(response);
            }

            return base.BadRequest(ModelState);
        }


        [HttpGet]
        [Route("Nodes(AgentId='{AgentId}')/Configurations(ConfigurationName='{ConfigurationName}')/ConfigurationContent")]
        public IActionResult GetConfiguration(GetConfigurationRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: MOF request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"AgentId=[{input.AgentId}] Configuration=[{input.ConfigurationName}]");
                
                FileContent configContent;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    configContent = h.GetConfiguration(input.AgentId, input.ConfigurationName);
                }
                if (configContent == null)
                    return NotFound();

                var response = new GetConfigurationResponse
                {
                    ChecksumAlgorithmHeader = configContent.ChecksumAlgorithm,
                    ChecksumHeader = configContent.Checksum,
                    Configuration = configContent.Content,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("Modules(ModuleName='{ModuleName}',ModuleVersion='{ModuleVersion}')/ModuleContent")]
        public IActionResult GetModule(GetModuleRequest input)
        {
            _logger.LogInformation("\n\n\nPOST: Module request");

            if (ModelState.IsValid)
            {
                _logger.LogDebug($"Module name=[{input.ModuleName}] Version=[{input.ModuleVersion}]");

                FileContent moduleContent;
                using (var h = _dscHandlerProvider.GetHandler(null))
                {
                    moduleContent = h.GetModule(input.ModuleName, input.ModuleVersion);
                }
                if (moduleContent == null)
                    return NotFound();

                var response = new GetModuleResponse
                {
                    ChecksumAlgorithmHeader = moduleContent.ChecksumAlgorithm,
                    ChecksumHeader = moduleContent.Checksum,
                    Module = moduleContent.Content,
                };

                return this.Model(response);
            }

            return BadRequest(ModelState);
        }
    }
}