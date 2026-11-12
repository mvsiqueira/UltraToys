# UltraToys — Pendências

## A fazer
- [ ] Janela "Sobre"
- [ ] Tema claro / escuro com troca ao vivo (F11/F12, como no UltraTask2) — hoje só existe o tema escuro
- [ ] Editor de atalho: hoje o Win+Ctrl+T do "Sempre no topo" é fixo, sem UI para trocar
- [ ] Guardar tamanho e posição da janela principal entre sessões (campos já existem em `AppSettings` mas não são aplicados/lidos no `MainWindow`)
- [ ] Ícone real do app na barra de título / Alt+Tab da janela principal (hoje usa `Resources\app-icon.ico`, validar em todos os tamanhos)
- [ ] "Colar sem formatação": restaurar a área de transferência original (com formatação) depois de colar — hoje ela fica só com o texto simples
- [ ] "Colar sem formatação": Win+Ctrl+V já é usado por outro programa/Windows em alguns PCs (o app avisa na seção); com o editor de atalho dá para trocar
- [ ] Quarta ferramenta (a definir)

## Feito ✓
- [x] Scaffold do projeto (WPF/.NET 10, MVVM + CommunityToolkit.Mvvm, estrutura Models/ViewModels/Views/Services)
- [x] App residente na bandeja (fechar a janela minimiza, não encerra o processo)
- [x] Interface `IUltraTool` para registrar novas ferramentas sem tocar no tray/app existente
- [x] "Sempre no topo": atalho global Win+Ctrl+T (HotkeyService via HwndSource) + lista de janelas com checkbox, sincronizados via `TopmostService`
- [x] Borda dourada de destaque ao redor de janelas fixadas (`BorderOverlayService`)
- [x] Re-fixar janelas ao reiniciar o app, por processo + título (`PinnedWindowMatcher`, melhor esforço)
- [x] "Mover apps para Monitor": cards de monitores na janela principal + submenu dinâmico na bandeja
- [x] Tratamento de janelas maximizadas/minimizadas ao mover, com janela sempre contida no monitor alvo
- [x] Detecção de monitores via `Screen.AllScreens` (mesmo padrão do UltraViagem)
- [x] Persistência de configurações em `settings.json` ao lado do executável
- [x] "Iniciar com o Windows" via `HKCU\...\Run`, autocorrigido se o app for movido/renomeado
- [x] Seções da janela principal retráteis (Expander com seta e cabeçalho clicável)
- [x] Seções em modo acordeão (expandir uma recolhe as outras); "Geral" no topo; ao abrir, todas ficam recolhidas
- [x] Seção "Geral" com "Iniciar com o Windows" e "Iniciar na bandeja" (abre sem mostrar a janela)
- [x] "Colar sem formatação": atalho global Win+Ctrl+V cola o texto sem formatação no app em foco (espera soltar as teclas, grava só texto na clipboard e envia Ctrl+V); aviso na janela se o atalho estiver ocupado
- [x] Ícone do app integrado (exportado do Ultrasoft, `dourado-14b/ultratoys-14b`)
- [x] Publicação single-file self-contained (`publish.ps1`)
- [x] Publicação framework-dependent via DLL + launcher VBS silencioso (`publish-dotnet.ps1`)
- [x] Testado de ponta a ponta: hotkey + checkbox, mover para monitor, minimizar/sair pela bandeja, build publicado
