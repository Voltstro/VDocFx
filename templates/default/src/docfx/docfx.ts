/*
 * Volt-DocfxV3-Theme
 *
 * Copyright (c) 2022 Voltstro-Studios
 * Copyright (c) 2020 docfx
 */
import 'bootstrap/js/dist/collapse'

import { enableAnchor } from './scripts/anchor'
import { highlight } from './scripts/highlight'
import { renderAlerts, renderLinks, renderTables, renderTabs } from './scripts/markdown'
import { renderAside, renderNavbar } from './scripts/nav'
import { renderToc } from './scripts/toc'

import './docfx.scss'

document.addEventListener('DOMContentLoaded', onContentLoad)

function onContentLoad() {
  enableAnchor()
  highlight()

  renderTables()
  renderAlerts()
  renderLinks()

  renderNavbar()
  renderToc()

  renderAside()
  renderTabs()
}