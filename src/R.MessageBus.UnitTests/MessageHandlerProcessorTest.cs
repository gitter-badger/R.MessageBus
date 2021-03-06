﻿//Copyright (C) 2015  Timothy Watson, Jakub Pachansky

//This program is free software; you can redistribute it and/or
//modify it under the terms of the GNU General Public License
//as published by the Free Software Foundation; either version 2
//of the License, or (at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.Collections.Generic;
using Moq;
using Newtonsoft.Json;
using R.MessageBus.Core;
using R.MessageBus.Interfaces;
using R.MessageBus.UnitTests.Fakes.Handlers;
using R.MessageBus.UnitTests.Fakes.Messages;
using Xunit;

namespace R.MessageBus.UnitTests
{
    public class MessageHandlerProcessorTest
    {
        private readonly Mock<IBusContainer> _mockContainer;

        public MessageHandlerProcessorTest()
        {
            _mockContainer = new Mock<IBusContainer>();
        }

        [Fact]
        public void ProcessMessageShouldGetTheCorrectHandlerTypesFromContainer()
        {
            // Arrange
            var messageProcessor = new MessageHandlerProcessor(_mockContainer.Object);

            // Act
            messageProcessor.ProcessMessage<FakeMessage1>(JsonConvert.SerializeObject(new FakeMessage1(Guid.NewGuid())
            {
                Username = "Tim Watson"
            }), null);

            // Assert
            _mockContainer.Verify(x => x.GetHandlerTypes(It.Is<Type>(y => y == typeof(IMessageHandler<FakeMessage1>))), Times.Once());
        }
       
        [Fact]
        public void ShouldExecuteTheCorrectHandlers()
        {
            // Arrange
            var messageProcessor = new MessageHandlerProcessor(_mockContainer.Object);

            var message1HandlerReference = new HandlerReference
            {
                HandlerType = typeof (FakeHandler1),
                MessageType = typeof (FakeMessage1)
            };


            _mockContainer.Setup(x => x.GetHandlerTypes(typeof(IMessageHandler<FakeMessage1>))).Returns(new List<HandlerReference>
            {
                message1HandlerReference
            });

            var fakeHandler = new FakeHandler1();
            _mockContainer.Setup(x => x.GetInstance(typeof (FakeHandler1))).Returns(fakeHandler);

            // Act
            var message1 = new FakeMessage1(Guid.NewGuid())
            {
                Username = "Tim Watson"
            };
            messageProcessor.ProcessMessage<FakeMessage1>(JsonConvert.SerializeObject(message1), null);

            var message2 = new FakeMessage2(Guid.NewGuid())
            {
                DisplayName = "Tim Watson"
            };

            messageProcessor.ProcessMessage<FakeMessage2>(JsonConvert.SerializeObject(message2), null);

            // Assert
            Assert.Equal(message1.CorrelationId, fakeHandler.Command.CorrelationId);
            Assert.Equal(message1.Username, fakeHandler.Command.Username);
            _mockContainer.Verify(x => x.GetInstance(typeof (FakeHandler2)), Times.Never);
        }
    }
}