using PubSub.Sample.OrderEventsSystem.AnalyticsService;
var analyticsService = new OrderEventsAnalyticsService();
await analyticsService.AnalyzeEvents();