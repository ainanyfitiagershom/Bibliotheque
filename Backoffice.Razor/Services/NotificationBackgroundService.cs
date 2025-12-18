using Bibliotheque.Core.Interfaces;

namespace Backoffice.Razor.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Exécuter toutes les heures

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service de notifications démarré.");

            // Exécuter immédiatement au démarrage
            await ExecuterTachesNotificationAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                    await ExecuterTachesNotificationAsync();
                }
                catch (OperationCanceledException)
                {
                    // Service arrêté normalement
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur dans le service de notifications.");
                }
            }

            _logger.LogInformation("Service de notifications arrêté.");
        }

        private async Task ExecuterTachesNotificationAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var empruntService = scope.ServiceProvider.GetRequiredService<IEmpruntService>();

                _logger.LogInformation("Exécution des tâches de notification...");

                // 1. Détecter et notifier les retards
                await empruntService.DetecterRetardsAsync();
                _logger.LogInformation("Détection des retards terminée.");

                // 2. Envoyer les rappels d'échéance (2 jours avant)
                await notificationService.EnvoyerRappelsEcheanceAsync(2);
                _logger.LogInformation("Envoi des rappels d'échéance terminé.");

                // 3. Nettoyer les anciennes notifications (plus de 30 jours)
                await notificationService.NettoyerAnciennesNotificationsAsync(30);
                _logger.LogInformation("Nettoyage des anciennes notifications terminé.");

                _logger.LogInformation("Tâches de notification terminées avec succès.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'exécution des tâches de notification.");
            }
        }
    }
}
