using PubSub.Sample.OrderEventsSystem.AuditService;
var auditService = new OrderEventsAuditService();
await auditService.AuditEvents();