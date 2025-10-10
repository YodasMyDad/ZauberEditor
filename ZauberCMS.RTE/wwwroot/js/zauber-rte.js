// Zauber RTE JavaScript Runtime
// This provides the browser-side functionality for the rich text editor

window.ZauberRTE = {
    // Shortcuts API
    shortcuts: {
        _registeredShortcuts: new Map(), // editorId -> Set of shortcut strings
        
        registerShortcuts: function(editorId, shortcuts) {
            this._registeredShortcuts.set(editorId, new Set(shortcuts));
        },
        
        shouldPreventDefault: function(editorId, event) {
            const shortcuts = this._registeredShortcuts.get(editorId);
            if (!shortcuts || shortcuts.size === 0) return false;
            
            // Parse the event into a shortcut string format (normalized to lowercase)
            const parts = [];
            if (event.ctrlKey || event.metaKey) parts.push('control');
            if (event.shiftKey) parts.push('shift');
            if (event.altKey) parts.push('alt');
            parts.push(event.key.toLowerCase());
            
            const eventShortcut = parts.join('+');
            
            // Check if any registered shortcut matches
            for (const shortcut of shortcuts) {
                const normalizedShortcut = shortcut.toLowerCase().replace('ctrl', 'control').replace('cmd', 'control');
                if (normalizedShortcut === eventShortcut) {
                    return true;
                }
            }
            
            return false;
        }
    },
    
    // Selection API
    selection: {
        _savedRanges: new Map(), // editorId -> saved range
        _trackedTags: new Map(), // editorId -> Set of tag names to track
        _tagNormalizationMap: new Map(), // editorId -> { tagName: primaryTag } (unified normalization)
        _blockTags: new Map(), // editorId -> Set of block-level tag names
        
        registerTrackedTags: function(editorId, tags) {
            this._trackedTags.set(editorId, new Set(tags.map(t => t.toLowerCase())));
        },
        
        registerTagNormalization: function(editorId, tagNormalizationMap) {
            // Normalize map keys to lowercase for case-insensitive lookups
            const normalizedMap = {};
            for (const [key, value] of Object.entries(tagNormalizationMap)) {
                normalizedMap[key.toLowerCase()] = value.toLowerCase();
            }
            this._tagNormalizationMap.set(editorId, normalizedMap);
        },
        
        registerBlockTags: function(editorId, blockTags) {
            this._blockTags.set(editorId, new Set(blockTags.map(t => t.toLowerCase())));
        },
        
        get: function(editorId) {
            const editor = document.getElementById(editorId);
            if (!editor) return null;

            const selection = window.getSelection();
            if (!selection.rangeCount) return null;

            const range = selection.getRangeAt(0);
            return {
                startOffset: range.startOffset,
                endOffset: range.endOffset,
                isCollapsed: range.collapsed,
                selectedText: selection.toString()
            };
        },
        
        saveRange: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) {
                return;
            }

            const selection = window.getSelection();
            if (!selection.rangeCount) {
                return;
            }

            const range = selection.getRangeAt(0);
            
            // Only save if the selection is within the editor
            if (editor.contains(range.commonAncestorContainer)) {
                this._savedRanges.set(editorId, range.cloneRange());
            } else {
            }
        },
        
        restoreRange: function(editorId) {
            const savedRange = this._savedRanges.get(editorId);
            if (!savedRange) {
                return false;
            }

            const selection = window.getSelection();
            selection.removeAllRanges();
            selection.addRange(savedRange);
            return true;
        },
        
        clearSavedRange: function(editorId) {
            this._savedRanges.delete(editorId);
        },

        getLinkAtCursor: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return null;

            const selection = window.getSelection();
            if (!selection.rangeCount) return null;

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;
            
            // If it's a text node, get the parent element
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Walk up the DOM to find an <a> tag
            while (element && element !== editor) {
                if (element.tagName && element.tagName.toLowerCase() === 'a') {
                    return {
                        href: element.getAttribute('href') || '',
                        target: element.getAttribute('target'),
                        rel: element.getAttribute('rel'),
                        title: element.getAttribute('title'),
                        text: element.textContent || '',
                        element: element
                    };
                }
                element = element.parentElement;
            }

            return null;
        },

        selectLinkAtCursor: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return false;

            const selection = window.getSelection();
            if (!selection.rangeCount) return false;

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;
            
            // If it's a text node, get the parent element
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Walk up the DOM to find an <a> tag
            while (element && element !== editor) {
                if (element.tagName && element.tagName.toLowerCase() === 'a') {
                    // Select the entire link element
                    const newRange = document.createRange();
                    newRange.selectNodeContents(element);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                    return true;
                }
                element = element.parentElement;
            }
            return false;
        },

        getImageAtCursor: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) {
                return null;
            }

            const selection = window.getSelection();
            if (!selection.rangeCount) {
                return null;
            }

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;
            
            // If it's a text node, get the parent element
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Walk up the DOM to find an <img> tag
            while (element && element !== editor) {
                if (element.tagName && element.tagName.toLowerCase() === 'img') {
                    const src = element.getAttribute('src') || '';
                    return {
                        src: src,
                        alt: element.getAttribute('alt'),
                        width: element.getAttribute('width') || element.style.width,
                        height: element.getAttribute('height') || element.style.height,
                        isDataUrl: src.startsWith('data:')
                    };
                }
                element = element.parentElement;
            }
            return null;
        },

        selectImageAtCursor: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) {
                return false;
            }

            const selection = window.getSelection();
            if (!selection.rangeCount) {
                return false;
            }

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;
            
            // If it's a text node, get the parent element
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Walk up the DOM to find an <img> tag
            while (element && element !== editor) {
                if (element.tagName && element.tagName.toLowerCase() === 'img') {
                    // Select the entire image element
                    const newRange = document.createRange();
                    newRange.selectNode(element);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                    return true;
                }
                element = element.parentElement;
            }
            return false;
        },

        set: function(editorId, startOffset, endOffset) {
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const selection = window.getSelection();
            const range = document.createRange();

            // This is a simplified implementation
            // In practice, you'd need more sophisticated range handling
            range.selectNodeContents(editor);
            range.setStart(editor.firstChild || editor, startOffset);
            range.setEnd(editor.firstChild || editor, endOffset);

            selection.removeAllRanges();
            selection.addRange(range);
        },

        collapse: function(editorId, toStart) {
            const selection = window.getSelection();
            if (selection.rangeCount > 0) {
                const range = selection.getRangeAt(0);
                range.collapse(toStart);
                selection.removeAllRanges();
                selection.addRange(range);
            }
        },

        wrap: function(editorId, tagName, attributes) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);

            // If range is collapsed (just a cursor), don't wrap anything
            if (range.collapsed) return;

            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // Create the wrapper element
            const element = document.createElement(tagName);

            if (attributes) {
                Object.keys(attributes).forEach(key => {
                    element.setAttribute(key, attributes[key]);
                });
            }

            try {
                // Try to use surroundContents first (works for simple cases)
                range.surroundContents(element);

                // Restore selection to the wrapped element
                range.selectNodeContents(element);
                selection.removeAllRanges();
                selection.addRange(range);
            } catch (e) {
                // surroundContents fails when range partially selects nodes or crosses element boundaries
                // Use a more robust approach: extract, wrap, and reinsert
                try {
                    const contents = range.extractContents();
                    element.appendChild(contents);
                    range.insertNode(element);

                    // Select the wrapped content
                    range.selectNodeContents(element);
                    selection.removeAllRanges();
                    selection.addRange(range);
                } catch (innerError) {
                    console.warn('Failed to wrap selection:', innerError);
                }
            }
        },

        unwrap: function(editorId, tagName) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // If range is collapsed, nothing to unwrap
            if (range.collapsed) return;

            // Find all elements of the specified type that intersect with the selection
            const elementsToUnwrap = [];
            const allElements = editor.querySelectorAll(tagName);

            allElements.forEach(element => {
                if (range.intersectsNode(element)) {
                    elementsToUnwrap.push(element);
                }
            });

            if (elementsToUnwrap.length === 0) return;

            // Sort by depth (deepest first) to unwrap from inside out
            elementsToUnwrap.sort((a, b) => {
                let depthA = 0, depthB = 0;
                let parentA = a.parentElement;
                let parentB = b.parentElement;

                while (parentA && parentA !== editor) {
                    depthA++;
                    parentA = parentA.parentElement;
                }
                while (parentB && parentB !== editor) {
                    depthB++;
                    parentB = parentB.parentElement;
                }

                return depthB - depthA;
            });

            // Track blocks for selection restoration
            const blockTags = this._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'li', 'td', 'th', 'ul', 'ol', 'table']);
            const affectedBlocks = new Set();

            // Unwrap each element
            elementsToUnwrap.forEach(element => {
                const parent = element.parentNode;
                if (!parent) return;

                // Track which block this belongs to
                let block = parent;
                while (block && block !== editor && !blockTags.has(block.tagName?.toLowerCase())) {
                    block = block.parentElement;
                }
                if (block && block !== editor) {
                    affectedBlocks.add(block);
                }

                // Collect child nodes before unwrapping
                const childNodes = Array.from(element.childNodes);
                
                // Insert all children before the element
                childNodes.forEach(child => {
                    parent.insertBefore(child, element);
                });
                
                // Remove the now-empty wrapper element
                parent.removeChild(element);
            });

            // Normalize all affected blocks to merge text nodes
            affectedBlocks.forEach(block => {
                block.normalize();
            });

            // Restore selection to span the affected blocks
            if (affectedBlocks.size > 0) {
                try {
                    const blocks = Array.from(affectedBlocks);
                    const firstBlock = blocks[0];
                    const lastBlock = blocks[blocks.length - 1];
                    
                    const newRange = document.createRange();
                    newRange.setStartBefore(firstBlock.firstChild || firstBlock);
                    newRange.setEndAfter(lastBlock.lastChild || lastBlock);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                } catch (e) {
                    console.warn('Could not restore selection after unwrap:', e);
                }
            }
        },

        handleEnterKey: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const contentEditable = document.getElementById(editorId + '-content');
            if (!contentEditable) return;

            // Get current block element
            let currentBlock = range.commonAncestorContainer;
            if (currentBlock.nodeType === Node.TEXT_NODE) {
                currentBlock = currentBlock.parentElement;
            }

            // Find the block-level parent
            const blockTags = this._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'li', 'td', 'th', 'ul', 'ol', 'table']);
            while (currentBlock && currentBlock !== contentEditable && !blockTags.has(currentBlock.tagName?.toLowerCase())) {
                currentBlock = currentBlock.parentElement;
            }

            if (currentBlock && currentBlock.tagName?.toLowerCase() === 'li') {
                // In a list item
                // Check if current list item is empty
                const isEmpty = !currentBlock.textContent?.trim() || 
                               currentBlock.innerHTML === '<br>' || 
                               currentBlock.innerHTML === '&nbsp;';
                
                if (isEmpty) {
                    // Empty list item - exit the list
                    const parentList = currentBlock.parentElement;
                    
                    // Check if this list is nested inside another list item
                    const grandParentLi = parentList.parentElement?.closest('li');
                    
                    if (grandParentLi) {
                        // We're in a nested list - create a new list item at parent level
                        const newLi = document.createElement('li');
                        newLi.innerHTML = '<br>';
                        
                        // Insert new list item after the grandparent li
                        const grandParentList = grandParentLi.parentElement;
                        grandParentList.insertBefore(newLi, grandParentLi.nextSibling);
                        
                        // Remove the empty nested list item
                        parentList.removeChild(currentBlock);
                        
                        // If nested list is now empty, remove it
                        if (parentList.children.length === 0) {
                            parentList.remove();
                        }
                        
                        // Move cursor to new list item
                        range.setStart(newLi, 0);
                        range.setEnd(newLi, 0);
                        selection.removeAllRanges();
                        selection.addRange(range);
                    } else {
                        // We're at top level - exit to paragraph
                        const newParagraph = document.createElement('p');
                        newParagraph.innerHTML = '<br>';
                        
                        // Insert paragraph after the list
                        parentList.parentNode.insertBefore(newParagraph, parentList.nextSibling);
                        
                        // Remove the empty list item
                        parentList.removeChild(currentBlock);
                        
                        // If list is now empty, remove it
                        if (parentList.children.length === 0) {
                            parentList.parentNode.removeChild(parentList);
                        }
                        
                        // Move cursor to new paragraph
                        range.setStart(newParagraph, 0);
                        range.setEnd(newParagraph, 0);
                        selection.removeAllRanges();
                        selection.addRange(range);
                    }
                } else {
                    // Non-empty list item - split it at cursor position
                    const splitRange = range.cloneRange();
                    splitRange.setEndAfter(currentBlock.lastChild || currentBlock);
                    const afterContent = splitRange.extractContents();
                    
                    // Create new list item with the content after cursor
                    const newLi = document.createElement('li');
                    newLi.appendChild(afterContent);
                    
                    // Clean both list items
                    this.cleanUnnecessarySpans(currentBlock);
                    this.cleanUnnecessarySpans(newLi);
                    
                    // Ensure both have content or br
                    if (!currentBlock.textContent?.trim() && currentBlock.children.length === 0) {
                        currentBlock.innerHTML = '<br>';
                    }
                    if (!newLi.textContent?.trim() && newLi.children.length === 0) {
                        newLi.innerHTML = '<br>';
                    }
                    
                    // Insert new list item after current one
                    currentBlock.parentNode.insertBefore(newLi, currentBlock.nextSibling);

                    // Move cursor to start of new list item
                    range.setStart(newLi, 0);
                    range.setEnd(newLi, 0);
                    selection.removeAllRanges();
                    selection.addRange(range);
                }
            } else {
                // Determine cursor position within the block
                let textBeforeCursor = '';
                let textAfterCursor = '';
                
                // Get all text content before and after cursor
                const walker = document.createTreeWalker(currentBlock, NodeFilter.SHOW_TEXT);
                let currentNode;
                let foundCursor = false;
                
                while (currentNode = walker.nextNode()) {
                    if (!foundCursor) {
                        if (currentNode === range.startContainer) {
                            // This is the node containing the cursor
                            textBeforeCursor += currentNode.textContent.substring(0, range.startOffset);
                            textAfterCursor += currentNode.textContent.substring(range.startOffset);
                            foundCursor = true;
                        } else {
                            textBeforeCursor += currentNode.textContent;
                        }
                    } else {
                        textAfterCursor += currentNode.textContent;
                    }
                }
                
                // Check if at the very beginning
                if (textBeforeCursor.length === 0) {
                    // Insert empty paragraph before
                    const emptyParagraph = document.createElement('p');
                    emptyParagraph.innerHTML = '<br>';
                    currentBlock.parentNode.insertBefore(emptyParagraph, currentBlock);
                    
                    // Focus stays in current paragraph
                    const newRange = document.createRange();
                    newRange.selectNodeContents(currentBlock);
                    newRange.collapse(true);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                    return;
                }
                
                // Split the paragraph - extract content after cursor
                const splitRange = range.cloneRange();
                splitRange.setEndAfter(currentBlock.lastChild || currentBlock);
                const afterContent = splitRange.extractContents();
                
                // Create new paragraph
                const newParagraph = document.createElement('p');
                newParagraph.appendChild(afterContent);
                
                // Clean both paragraphs
                this.cleanUnnecessarySpans(currentBlock);
                this.cleanUnnecessarySpans(newParagraph);
                
                // Ensure both have content or br
                if (!currentBlock.textContent?.trim() && currentBlock.children.length === 0) {
                    currentBlock.innerHTML = '<br>';
                }
                if (!newParagraph.textContent?.trim() && newParagraph.children.length === 0) {
                    newParagraph.innerHTML = '<br>';
                }
                
                // Insert new paragraph after current
                currentBlock.parentNode.insertBefore(newParagraph, currentBlock.nextSibling);
                
                // Move cursor to start of new paragraph
                const newRange = document.createRange();
                newRange.selectNodeContents(newParagraph);
                newRange.collapse(true);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }
        },

        insertHtml: function(editorId, html) {
            // Get the editor content element
            const editor = document.getElementById(editorId + '-content');
            if (!editor) {
                return;
            }

            // Focus the editor first to ensure selection is in the right place
            editor.focus();
            
            const selection = window.getSelection();
            let range;
            if (!selection.rangeCount || !editor.contains(selection.anchorNode)) {
                // Create a new range at the end of the editor
                range = document.createRange();
                const lastChild = editor.lastChild;
                if (lastChild) {
                    range.setStartAfter(lastChild);
                    range.setEndAfter(lastChild);
                } else {
                    range.selectNodeContents(editor);
                    range.collapse(false);
                }
                selection.removeAllRanges();
                selection.addRange(range);
            } else {
                range = selection.getRangeAt(0);
            }

            // Check if we're pasting block-level content
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;
            const blockTags = ['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'ul', 'ol', 'table', 'hr'];
            const hasBlockContent = Array.from(tempDiv.children).some(child => 
                blockTags.includes(child.tagName.toLowerCase())
            );

            // If pasting block content and we're inside a block element, handle specially
            if (hasBlockContent) {
                // Find the closest block element
                let blockParent = range.commonAncestorContainer;
                if (blockParent.nodeType === Node.TEXT_NODE) {
                    blockParent = blockParent.parentElement;
                }
                
                while (blockParent && blockParent !== editor && 
                       !blockTags.includes(blockParent.tagName?.toLowerCase())) {
                    blockParent = blockParent.parentElement;
                }

                // If we're inside a block element (not the editor root), replace or split it
                if (blockParent && blockParent !== editor && blockTags.includes(blockParent.tagName?.toLowerCase())) {
                    // Check if we're replacing all content in the block BEFORE deleting
                    const selectedText = range.toString();
                    const blockText = blockParent.textContent;
                    const isReplacingAllContent = !range.collapsed && (selectedText === blockText || blockText.trim() === selectedText.trim());
                    
                    // Delete selected content
                    range.deleteContents();
                    
                    // If we're replacing all content in the block or the block is now empty, replace the entire block
                    const remainingText = blockParent.textContent.trim();
                    if (isReplacingAllContent || !remainingText) {
                        // Replace the entire block element with the pasted block(s)
                        const fragment = document.createDocumentFragment();
                        const nodesToInsert = Array.from(tempDiv.children);
                        nodesToInsert.forEach(child => {
                            fragment.appendChild(child.cloneNode(true));
                        });
                        
                        // Get reference to the parent and next sibling before replacing
                        const parentNode = blockParent.parentNode;
                        const nextSibling = blockParent.nextSibling;
                        
                        // Remove the old block
                        blockParent.remove();
                        
                        // Insert all new nodes
                        const insertedNodes = [];
                        while (fragment.firstChild) {
                            const node = fragment.firstChild;
                            if (nextSibling) {
                                parentNode.insertBefore(node, nextSibling);
                            } else {
                                parentNode.appendChild(node);
                            }
                            insertedNodes.push(node);
                        }
                        
                        // Position cursor at end of the last inserted element
                        if (insertedNodes.length > 0) {
                            const lastNode = insertedNodes[insertedNodes.length - 1];
                            const newRange = document.createRange();
                            newRange.selectNodeContents(lastNode);
                            newRange.collapse(false);
                            selection.removeAllRanges();
                            selection.addRange(newRange);
                        }
                        return;
                    } else {
                        // Split the block at the insertion point and insert blocks between splits
                        const parentNode = blockParent.parentNode;
                        const insertionPoint = range.startContainer;
                        const insertionOffset = range.startOffset;
                        
                        // Extract content after cursor into a new block
                        const afterRange = document.createRange();
                        afterRange.setStart(insertionPoint, insertionOffset);
                        afterRange.setEndAfter(blockParent.lastChild || blockParent);
                        const afterContent = afterRange.extractContents();
                        
                        // Only create after-block if there's meaningful content
                        const afterBlock = afterContent.textContent.trim() ? 
                            blockParent.cloneNode(false) : null;
                        if (afterBlock) {
                            afterBlock.appendChild(afterContent);
                        }
                        
                        // Insert the new block elements
                        const nodesToInsert = Array.from(tempDiv.children);
                        const insertedNodes = [];
                        let insertAfter = blockParent;
                        
                        nodesToInsert.forEach(child => {
                            const newNode = child.cloneNode(true);
                            parentNode.insertBefore(newNode, insertAfter.nextSibling);
                            insertAfter = newNode;
                            insertedNodes.push(newNode);
                        });
                        
                        // Insert the after-block if it has content
                        if (afterBlock) {
                            parentNode.insertBefore(afterBlock, insertAfter.nextSibling);
                        }
                        
                        // Position cursor at start of after-block, or end of last inserted node
                        const newRange = document.createRange();
                        if (afterBlock) {
                            newRange.setStart(afterBlock, 0);
                            newRange.collapse(true);
                        } else if (insertedNodes.length > 0) {
                            const lastNode = insertedNodes[insertedNodes.length - 1];
                            newRange.selectNodeContents(lastNode);
                            newRange.collapse(false);
                        }
                        selection.removeAllRanges();
                        selection.addRange(newRange);
                        return;
                    }
                }
            }

            // Standard insertion for inline content or when not in a block context
            range.deleteContents();
            const fragment = range.createContextualFragment(html);
            range.insertNode(fragment);
            
            // Move cursor to end of inserted content
            range.collapse(false);
            selection.removeAllRanges();
            selection.addRange(range);
        },

        getActiveMarks: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return [];

            const range = selection.getRangeAt(0);
            const marks = new Set();
            const trackedTags = this._trackedTags.get(editorId);
            
            if (!trackedTags || trackedTags.size === 0) {
                return [];
            }

            // Check for marks at the current position
            let element = range.commonAncestorContainer;
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Walk up the DOM to find active marks dynamically
            while (element && element.id !== editorId) {
                const tagName = element.tagName ? element.tagName.toLowerCase() : '';
                
                // Check if this tag is being tracked by any toolbar item
                if (tagName && trackedTags.has(tagName)) {
                    // Normalize certain tags to their canonical form
                    const normalizedTag = this._normalizeTagName(editorId, tagName);
                    marks.add(normalizedTag);
                }
                
                element = element.parentElement;
            }

            return Array.from(marks);
        },
        
        _normalizeTagName: function(editorId, tagName) {
            // Normalize tag names to their canonical form using registered map
            // E.g., "b" -> "strong", "strong" -> "strong", "i" -> "em", "em" -> "em"
            const tagMap = this._tagNormalizationMap.get(editorId) || {};
            return tagMap[tagName.toLowerCase()] || tagName;
        },

        hasMarkInSelection: function(editorId, markName) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return false;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId);
            if (!editor) return false;

            // If range is collapsed, check the current position
            if (range.collapsed) {
                const activeMarks = this.getActiveMarks(editorId);
                return activeMarks.includes(markName);
            }

            // For non-collapsed selections, check if any part contains the mark
            const tagName = this.getTagForMark(editorId, markName);
            const container = range.commonAncestorContainer;
            
            // Get the container element
            let containerElement = container;
            if (containerElement.nodeType === Node.TEXT_NODE) {
                containerElement = containerElement.parentElement;
            }

            // Check if the container itself is the mark
            if (containerElement.tagName && containerElement.tagName.toLowerCase() === tagName) {
                return true;
            }

            // Find all mark elements within the container
            const markElements = containerElement.querySelectorAll(tagName);
            
            // Check if any of these elements intersect with the selection
            for (let element of markElements) {
                if (range.intersectsNode(element)) {
                    return true;
                }
            }

            // Also check if the selection is fully within a mark by walking up the tree
            let element = containerElement;
            while (element && element.id !== editorId) {
                if (element.tagName && element.tagName.toLowerCase() === tagName) {
                    return true;
                }
                element = element.parentElement;
            }

            return false;
        },

        getTagForMark: function(editorId, markName) {
            // Mark names are already tag names (e.g., "strong", "em")
            // Just normalize them to handle any edge cases
            return this._normalizeTagName(editorId, markName);
        },

        getCurrentBlockType: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return 'paragraph';

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;

            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Find the block-level element
            while (element && element.id !== editorId) {
                const tagName = element.tagName ? element.tagName.toLowerCase() : '';
                const blockTags = this._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'li', 'td', 'th', 'ul', 'ol', 'table']);

                if (blockTags.has(tagName)) {
                    // Handle heading levels
                    if (tagName.startsWith('h') && tagName.length === 2) {
                        const level = parseInt(tagName.substring(1));
                        if (level >= 1 && level <= 6) {
                            // Store heading level for later retrieval
                            element._headingLevel = level;
                            return 'heading';
                        }
                        return 'paragraph';
                    }

                    // Handle special cases
                    switch (tagName) {
                        case 'pre':
                            return 'pre';
                        case 'li':
                            // Check parent for list type
                            const parent = element.parentElement;
                            return parent && parent.tagName.toLowerCase() === 'ol' ? 'ol' : 'ul';
                        default:
                            return tagName;
                    }
                }

                element = element.parentElement;
            }

            return 'paragraph';
        },

        getCurrentHeadingLevel: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return 0;

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;

            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            // Find the heading element
            while (element && element.id !== editorId) {
                const tagName = element.tagName ? element.tagName.toLowerCase() : '';

                if (tagName.startsWith('h') && tagName.length === 2) {
                    const level = parseInt(tagName.substring(1));
                    return level >= 1 && level <= 6 ? level : 0;
                }

                element = element.parentElement;
            }

            return 0;
        },

        selectMarkAtCursor: function(editorId, markName) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) {
                return false;
            }

            const selection = window.getSelection();
            if (!selection.rangeCount) {
                return false;
            }

            const range = selection.getRangeAt(0);
            
            // Only proceed if range is collapsed (cursor only)
            if (!range.collapsed) {
                return false;
            }

            let element = range.commonAncestorContainer;
            
            // If it's a text node, get the parent element
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            const tagName = this.getTagForMark(editorId, markName);

            // Walk up the DOM to find the mark tag
            while (element && element !== editor) {
                if (element.tagName && element.tagName.toLowerCase() === tagName) {
                    // Select the entire mark element
                    const newRange = document.createRange();
                    newRange.selectNodeContents(element);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                    return true;
                }
                element = element.parentElement;
            }
            return false;
        },

        toggleMark: function(editorId, markName) {
            const editor = document.getElementById(editorId);
            if (!editor) {
                return;
            }

            // Focus the editor to ensure commands work
            editor.focus();

            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);

            // Check if the mark exists anywhere in the selection
            const hasMarkInSelection = this.hasMarkInSelection(editorId, markName);
            if (hasMarkInSelection) {
                // REMOVING the mark
                // If range is collapsed (cursor only) and we're inside a mark, select it first
                if (range.collapsed) {
                    this.selectMarkAtCursor(editorId, markName);
                }
                
                // Remove all instances of the mark from the selection
                this.unwrap(editorId, this.getTagForMark(editorId, markName));
                // Clean up any spans that might have been created during unwrapping
                const editor = document.getElementById(editorId);
                if (editor) {
                    this.cleanUnnecessarySpans(editor);
                }
            } else {
                // ADDING the mark
                // Only apply if there's an actual selection (not just a cursor)
                if (range.collapsed) {
                    return;
                }
                
                // Apply the mark to the selection
                this.wrap(editorId, this.getTagForMark(editorId, markName));
            }
        },

        removeMark: function(editorId, markName) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const tagName = this.getTagForMark(editorId, markName);

            // If range is collapsed, try to find and remove marks at cursor position
            if (range.collapsed) {
                let element = range.startContainer;
                if (element.nodeType === Node.TEXT_NODE) {
                    element = element.parentElement;
                }

                // Look for the mark element containing the cursor
                while (element && element !== editor) {
                    if (element.tagName && element.tagName.toLowerCase() === tagName) {
                        // Found the mark element - unwrap it
                        const parent = element.parentNode;
                        while (element.firstChild) {
                            parent.insertBefore(element.firstChild, element);
                        }
                        parent.removeChild(element);
                        return;
                    }
                    element = element.parentElement;
                }
                return; // No mark found at cursor
            }

            // Range is not collapsed - extract and clean the selected content
            const contents = range.extractContents();
            const tempDiv = document.createElement('div');
            tempDiv.appendChild(contents);

            // Remove all instances of the mark tag
            const markElements = tempDiv.querySelectorAll(tagName);
            markElements.forEach(element => {
                // Replace the element with its contents
                const parent = element.parentNode;
                while (element.firstChild) {
                    parent.insertBefore(element.firstChild, element);
                }
                parent.removeChild(element);
            });

            // Insert the cleaned content back
            const insertedNodes = [];
            while (tempDiv.firstChild) {
                const node = tempDiv.firstChild;
                range.insertNode(node);
                insertedNodes.push(node);
                // Move range to after the inserted node for next insertion
                range.setStartAfter(node);
                range.setEndAfter(node);
            }

            // Restore selection to the cleaned content
            if (insertedNodes.length > 0) {
                const newRange = document.createRange();
                newRange.setStartBefore(insertedNodes[0]);
                newRange.setEndAfter(insertedNodes[insertedNodes.length - 1]);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }
        },

        getCommandForMark: function(markName) {
            const commandMap = {
                'strong': 'bold',
                'em': 'italic',
                'u': 'underline',
                's': 'strikethrough',
                'sub': 'subscript',
                'sup': 'superscript'
            };
            return commandMap[markName] || markName;
        },

        getSelectedListItems: function(range, editor) {
            const selectedLis = [];

            // Find all li elements that intersect with the selection
            const allLis = editor.querySelectorAll('li');
            allLis.forEach(li => {
                if (range.intersectsNode(li)) {
                    selectedLis.push(li);
                }
            });

            // Filter out parent list items if their children are also selected
            // We only want the "deepest" list items that are actually selected
            const filteredLis = selectedLis.filter(li => {
                // Check if any of this li's children are also in the selected list
                const hasSelectedChild = selectedLis.some(otherLi => 
                    otherLi !== li && li.contains(otherLi)
                );
                // Only include this li if none of its children are selected
                return !hasSelectedChild;
            });

            return filteredLis;
        },

        convertSingleBlock: function(block, targetType, attributes, blockTags) {
            if (!block || !block.parentNode) return null;

            const currentType = block.tagName?.toLowerCase();
            
            // Handle heading comparisons specially (current='h1', target='heading' with level='1')
            if (targetType === 'heading' && currentType.match(/^h[1-6]$/)) {
                const currentLevel = currentType.substring(1);
                const targetLevel = attributes?.level || '1';
                if (currentLevel === targetLevel) {
                    // Same heading level - toggle to paragraph
                    targetType = 'p';
                }
                // Otherwise, fall through to convert to different heading level
            }
            // Check if we're trying to convert to the exact same type
            else if (currentType === targetType) {
                // For blockquote, pre - toggle back to paragraph
                if (currentType === 'blockquote' || currentType === 'pre') {
                    targetType = 'p';
                } else if (currentType === 'p') {
                    // Already paragraph, no change needed
                    return block;
                } else {
                    // Already the target type and no special toggle behavior
                    return block;
                }
            }

            // Handle list items specially
            if (currentType === 'li') {
                const parentList = block.parentElement;
                if (!parentList) return block;
                
                const listType = parentList.tagName?.toLowerCase();
                
                // If trying to convert to the same list type, convert to paragraph or promote
                if (targetType === listType) {
                    // Check if this is a nested list (parent of list is an LI)
                    const parentListItem = parentList.parentElement;
                    const isNestedList = parentListItem?.tagName?.toLowerCase() === 'li';
                    
                    if (isNestedList) {
                        // For nested lists, promote the item to the parent list level
                        const grandparentList = parentListItem.parentElement; // The top-level UL/OL
                        
                        // Create a new list item at the parent level with the content
                        const promotedLi = document.createElement('li');
                        while (block.firstChild) {
                            promotedLi.appendChild(block.firstChild);
                        }
                        
                        // Insert the promoted item right after the parent list item
                        if (grandparentList && parentListItem.nextSibling) {
                            grandparentList.insertBefore(promotedLi, parentListItem.nextSibling);
                        } else if (grandparentList) {
                            grandparentList.appendChild(promotedLi);
                        }
                        
                        // Remove the original nested item
                        block.remove();
                        
                        // If the nested list is now empty, remove it
                        if (parentList.children.length === 0) {
                            parentList.remove();
                        }
                        
                        return promotedLi;
                    }
                    
                    // Not a nested list - normal conversion to paragraph
                    const newParagraph = document.createElement('p');
                    while (block.firstChild) {
                        newParagraph.appendChild(block.firstChild);
                    }
                    
                    if (parentList.children.length === 1) {
                        // Last item in list - replace entire list
                        parentList.parentNode.replaceChild(newParagraph, parentList);
                    } else {
                        // Multiple items - insert paragraph and remove this li
                        parentList.parentNode.insertBefore(newParagraph, parentList);
                        parentList.removeChild(block);
                    }
                    return newParagraph;
                }
                
                // Converting to different list type - convert the parent list
                if (targetType === 'ul' || targetType === 'ol') {
                    const newList = document.createElement(targetType);
                    while (parentList.firstChild) {
                        newList.appendChild(parentList.firstChild);
                    }
                    parentList.parentNode.replaceChild(newList, parentList);
                    return newList.querySelector('li'); // Return the converted li element
                }
                
                return block;
            }

            // Handle converting paragraphs to lists
            if ((targetType === 'ul' || targetType === 'ol') && blockTags.has(currentType)) {
                const newList = document.createElement(targetType);
                const li = document.createElement('li');
                
                // Move CONTENT from block to li (unwrap any nested block tags)
                while (block.firstChild) {
                    const child = block.firstChild;
                    // If child is a block element (like nested p tags), unwrap it
                    if (child.nodeType === Node.ELEMENT_NODE && blockTags.has(child.tagName?.toLowerCase())) {
                        // Extract content from nested block
                        while (child.firstChild) {
                            li.appendChild(child.firstChild);
                        }
                        block.removeChild(child);
                    } else {
                        li.appendChild(child);
                    }
                }
                
                newList.appendChild(li);
                block.parentNode.replaceChild(newList, block);
                return newList;
            }

            // Standard block conversion (p, h1-h6, blockquote, etc.)
            let newBlock;
            if (targetType === 'heading') {
                const level = attributes?.level || '1';
                newBlock = document.createElement(`h${level}`);
            } else if (targetType === 'codeblock') {
                newBlock = document.createElement('pre');
                const code = document.createElement('code');
                newBlock.appendChild(code);
            } else {
                newBlock = document.createElement(targetType || 'p');
            }

            // Copy attributes
            if (attributes) {
                Object.keys(attributes).forEach(key => {
                    if (key !== 'level') { // Skip 'level' as it's handled above
                        newBlock.setAttribute(key, attributes[key]);
                    }
                });
            }

            // Move content
            if (currentType === 'pre') {
                // Extract from code element if present
                const codeElement = block.querySelector('code');
                if (codeElement) {
                    while (codeElement.firstChild) {
                        if (targetType === 'codeblock' && newBlock.firstChild) {
                            newBlock.firstChild.appendChild(codeElement.firstChild);
                        } else {
                            newBlock.appendChild(codeElement.firstChild);
                        }
                    }
                } else {
                    while (block.firstChild) {
                        newBlock.appendChild(block.firstChild);
                    }
                }
            } else {
                // Normal content transfer
                while (block.firstChild) {
                    if (targetType === 'codeblock' && newBlock.firstChild) {
                        newBlock.firstChild.appendChild(block.firstChild);
                    } else {
                        newBlock.appendChild(block.firstChild);
                    }
                }
            }

            // Replace old block with new block
            block.parentNode.replaceChild(newBlock, block);
            return newBlock;
        },

        convertSelectedListItems: function(selectedLis, blockType, range, selection) {
            if (selectedLis.length === 0) return;

            // Group list items by their parent list using a unique identifier
            const listGroups = new Map();
            selectedLis.forEach(li => {
                const parentList = li.parentElement;
                if (parentList) {
                    if (!listGroups.has(parentList)) {
                        listGroups.set(parentList, []);
                    }
                    listGroups.get(parentList).push(li);
                }
            });

            // Check if we're converting to the same list type (should convert to paragraphs)
            const shouldConvertToParagraphs = listGroups.size > 0 && Array.from(listGroups.keys()).every(list =>
                list.tagName?.toLowerCase() === blockType
            );

            if (shouldConvertToParagraphs) {
                // Convert to paragraphs
                const newParagraphs = [];

                // Process each list group
                listGroups.forEach((lisInList, list) => {
                    const allLisInList = Array.from(list.children).filter(child => child.tagName?.toLowerCase() === 'li');
                    const parentListItem = list.parentElement;
                    const isNestedList = parentListItem?.tagName?.toLowerCase() === 'li';

                    // For nested lists, promote items to parent level
                    if (isNestedList) {
                        const grandparentList = parentListItem.parentElement; // The top-level UL/OL
                        
                        if (grandparentList) {
                            // Promote each selected nested item to the parent level
                            lisInList.forEach(li => {
                                // Create a new list item at the parent level
                                const promotedLi = document.createElement('li');
                                while (li.firstChild) {
                                    promotedLi.appendChild(li.firstChild);
                                }
                                
                                // Insert after the parent list item
                                if (parentListItem.nextSibling) {
                                    grandparentList.insertBefore(promotedLi, parentListItem.nextSibling);
                                } else {
                                    grandparentList.appendChild(promotedLi);
                                }
                                
                                // Remove the original
                                li.remove();
                            });
                            
                            // If the nested list is now empty, remove it
                            if (list.children.length === 0) {
                                list.remove();
                            }
                        }
                        return;
                    }

                    // Not a nested list - normal conversion to paragraphs
                    // If all list items in the list are selected, replace the entire list
                    if (lisInList.length === allLisInList.length) {
                        // Create paragraphs for each li and replace the whole list
                        const fragment = document.createDocumentFragment();
                        lisInList.forEach(li => {
                            const paragraph = document.createElement('p');
                            while (li.firstChild) {
                                paragraph.appendChild(li.firstChild);
                            }
                            newParagraphs.push(paragraph);
                            fragment.appendChild(paragraph);
                        });
                        list.parentNode.replaceChild(fragment, list);
                    } else {
                        // Only some list items are selected - replace them individually
                        lisInList.forEach(li => {
                            const paragraph = document.createElement('p');
                            while (li.firstChild) {
                                paragraph.appendChild(li.firstChild);
                            }
                            newParagraphs.push(paragraph);
                            list.parentNode.insertBefore(paragraph, li);
                            list.removeChild(li);
                        });
                    }
                });

                // Restore selection to cover all the new paragraphs
                if (newParagraphs.length > 0) {
                    const newRange = document.createRange();
                    newRange.setStartBefore(newParagraphs[0]);
                    newRange.setEndAfter(newParagraphs[newParagraphs.length - 1]);
                    selection.removeAllRanges();
                    selection.addRange(newRange);
                }
            } else {
                // Convert to different list type - handle this by changing parent list types
                listGroups.forEach((lisInList, list) => {
                    const allLisInList = Array.from(list.children).filter(child => child.tagName?.toLowerCase() === 'li');

                    if (lisInList.length === allLisInList.length) {
                        // All items selected - change the entire list type
                        const newList = document.createElement(blockType);
                        // Copy all attributes
                        for (let attr of list.attributes) {
                            newList.setAttribute(attr.name, attr.value);
                        }
                        // Move all children
                        while (list.firstChild) {
                            newList.appendChild(list.firstChild);
                        }
                        // Replace the old list
                        list.parentNode.replaceChild(newList, list);
                    } else {
                        // Partial selection - this is complex, for now just change the parent list type
                        const newList = document.createElement(blockType);
                        // Copy all attributes
                        for (let attr of list.attributes) {
                            newList.setAttribute(attr.name, attr.value);
                        }
                        // Move all children
                        while (list.firstChild) {
                            newList.appendChild(list.firstChild);
                        }
                        // Replace the old list
                        list.parentNode.replaceChild(newList, list);
                    }
                });
            }
        },

        setBlockType: function(editorId, blockType, attributes) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // Normalize heading tags (h1, h2, etc.) to internal format
            if (blockType && blockType.match(/^h[1-6]$/)) {
                const level = blockType.substring(1);
                attributes = attributes || {};
                attributes.level = level;
                blockType = 'heading';
            }

            // Check if selection spans multiple elements and contains list items
            if (!range.collapsed) {
                const selectedLis = this.getSelectedListItems(range, editor);
                if (selectedLis.length > 0) {
                    // Handle multi-list-item selection
                    this.convertSelectedListItems(selectedLis, blockType, range, selection);
                    return;
                }
            }

            const blockTags = this._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'li', 'td', 'th', 'ul', 'ol', 'table']);

            // Find the current block element
            let currentBlock = range.commonAncestorContainer;
            if (currentBlock.nodeType === Node.TEXT_NODE) {
                currentBlock = currentBlock.parentElement;
            }

            // Check if selection spans multiple block elements at the root level
            if (currentBlock && currentBlock.id === (editorId + '-content') && !range.collapsed) {
                // Selection spans multiple blocks - handle each block separately
                const affectedBlocks = [];
                const allBlocks = editor.querySelectorAll(Array.from(blockTags).join(','));
                
                allBlocks.forEach(block => {
                    if (range.intersectsNode(block)) {
                        affectedBlocks.push(block);
                    }
                });

                if (affectedBlocks.length > 0) {
                    // Special handling for list conversion - merge into a single list
                    if (blockType === 'ul' || blockType === 'ol') {
                        const newList = document.createElement(blockType);
                        
                        affectedBlocks.forEach(block => {
                            const li = document.createElement('li');
                            
                            // Extract content from block, unwrapping any nested blocks
                            while (block.firstChild) {
                                const child = block.firstChild;
                                if (child.nodeType === Node.ELEMENT_NODE && blockTags.has(child.tagName?.toLowerCase())) {
                                    // Unwrap nested block elements
                                    while (child.firstChild) {
                                        li.appendChild(child.firstChild);
                                    }
                                    block.removeChild(child);
                                } else {
                                    li.appendChild(child);
                                }
                            }
                            
                            newList.appendChild(li);
                        });
                        
                        // Replace the first affected block with the list
                        affectedBlocks[0].parentNode.replaceChild(newList, affectedBlocks[0]);
                        
                        // Remove the remaining affected blocks
                        for (let i = 1; i < affectedBlocks.length; i++) {
                            if (affectedBlocks[i].parentNode) {
                                affectedBlocks[i].parentNode.removeChild(affectedBlocks[i]);
                            }
                        }
                        
                        // Restore selection to the new list
                        const newRange = document.createRange();
                        newRange.selectNodeContents(newList);
                        selection.removeAllRanges();
                        selection.addRange(newRange);
                        return;
                    }
                    
                    // For non-list conversions, convert each block individually
                    const convertedBlocks = [];
                    
                    affectedBlocks.forEach(block => {
                        const converted = this.convertSingleBlock(block, blockType, attributes, blockTags);
                        if (converted) {
                            convertedBlocks.push(converted);
                        }
                    });

                    // Restore selection across all converted blocks
                    if (convertedBlocks.length > 0) {
                        const newRange = document.createRange();
                        newRange.setStartBefore(convertedBlocks[0]);
                        newRange.setEndAfter(convertedBlocks[convertedBlocks.length - 1]);
                        selection.removeAllRanges();
                        selection.addRange(newRange);
                    }
                    return;
                }
            }

            // Find the specific block containing the selection
            while (currentBlock && currentBlock.id !== (editorId + '-content') && !blockTags.has(currentBlock.tagName?.toLowerCase())) {
                currentBlock = currentBlock.parentElement;
            }

            if (!currentBlock || currentBlock.id === (editorId + '-content')) {
                // No block found or at editor root with collapsed selection
                if (range.collapsed) {
                    // Create a paragraph for collapsed cursor at root
                    currentBlock = document.createElement('p');
                    range.insertNode(currentBlock);
                    range.selectNodeContents(currentBlock);
                } else {
                    // Should not reach here, but handle gracefully
                    return;
                }
            }

            // Handle special case: converting from list item
            if (currentBlock.tagName?.toLowerCase() === 'li') {
                const parentList = currentBlock.parentElement;
                if (parentList && (parentList.tagName?.toLowerCase() === 'ul' || parentList.tagName?.toLowerCase() === 'ol')) {
                    // If converting to the same list type, convert to paragraph
                    if (blockType === parentList.tagName.toLowerCase()) {
                        // Extract content from li and create paragraph
                        const newParagraph = document.createElement('p');
                        while (currentBlock.firstChild) {
                            newParagraph.appendChild(currentBlock.firstChild);
                        }

                        // Replace the entire list with the paragraph if it's the only item
                        if (parentList.children.length === 1) {
                            parentList.parentNode.replaceChild(newParagraph, parentList);
                        } else {
                            // Replace just this li with the paragraph
                            parentList.parentNode.insertBefore(newParagraph, parentList);
                            parentList.removeChild(currentBlock);
                        }

                        // Restore selection to the new paragraph
                        range.selectNodeContents(newParagraph);
                        selection.removeAllRanges();
                        selection.addRange(range);
                        return;
                    }
                    // If converting to different list type, change the parent list type
                    else if (blockType === 'ul' || blockType === 'ol') {
                        const newList = document.createElement(blockType);
                        // Copy all attributes
                        for (let attr of parentList.attributes) {
                            newList.setAttribute(attr.name, attr.value);
                        }
                        // Move all children
                        while (parentList.firstChild) {
                            newList.appendChild(parentList.firstChild);
                        }
                        // Replace the old list
                        parentList.parentNode.replaceChild(newList, parentList);
                        // Selection should already be correct
                        return;
                    }
                }
            }

            // Create the new block element
            let newBlock;
            if (blockType === 'heading') {
                const level = attributes?.level || '1';
                newBlock = document.createElement(`h${level}`);
            } else if (blockType === 'codeblock') {
                newBlock = document.createElement('pre');
                const code = document.createElement('code');
                newBlock.appendChild(code);
            } else if (blockType === 'blockquote') {
                newBlock = document.createElement('blockquote');
            } else if (blockType === 'ul' || blockType === 'ol') {
                newBlock = document.createElement(blockType);
                const li = document.createElement('li');
                newBlock.appendChild(li);
            } else {
                newBlock = document.createElement(blockType || 'p');
            }

            // Copy attributes
            if (attributes) {
                Object.keys(attributes).forEach(key => {
                    newBlock.setAttribute(key, attributes[key]);
                });
            }

            // Move content to new block
            let hasContent = false;
            
            // Special handling when converting FROM pre to p - extract text from code tag
            if (currentBlock.tagName?.toLowerCase() === 'pre' && blockType === 'p') {
                const codeElement = currentBlock.querySelector('code');
                if (codeElement) {
                    // Extract text content from code element
                    const textContent = codeElement.textContent || '';
                    newBlock.textContent = textContent;
                    hasContent = textContent.length > 0;
                } else {
                    // No code element, just move content
                    while (currentBlock.firstChild) {
                        hasContent = true;
                        newBlock.appendChild(currentBlock.firstChild);
                    }
                }
            } else {
                // Normal content transfer
                while (currentBlock.firstChild) {
                    hasContent = true;
                    if (newBlock.tagName.toLowerCase() === 'pre' && newBlock.firstChild) {
                        // For code blocks, move content into the code element
                        newBlock.firstChild.appendChild(currentBlock.firstChild);
                    } else if (newBlock.tagName.toLowerCase() === 'ul' || newBlock.tagName.toLowerCase() === 'ol') {
                        // For lists, move content into the li element
                        newBlock.firstChild.appendChild(currentBlock.firstChild);
                    } else {
                        newBlock.appendChild(currentBlock.firstChild);
                    }
                }
            }

            // If creating a list and there's no content, add br to the li element
            if ((newBlock.tagName.toLowerCase() === 'ul' || newBlock.tagName.toLowerCase() === 'ol') && !hasContent) {
                newBlock.firstChild.innerHTML = '<br>';
            }

            // Replace the old block
            currentBlock.parentNode.replaceChild(newBlock, currentBlock);

            // Restore selection
            let selectionTarget = newBlock;
            if (newBlock.tagName.toLowerCase() === 'pre' && newBlock.firstChild) {
                // For code blocks, select the code element
                selectionTarget = newBlock.firstChild;
            } else if (newBlock.tagName.toLowerCase() === 'ul' || newBlock.tagName.toLowerCase() === 'ol') {
                // For lists, select the li element
                selectionTarget = newBlock.firstChild;
            }
            range.selectNodeContents(selectionTarget);
            selection.removeAllRanges();
            selection.addRange(range);
        },

        setBlockStyle: function(editorId, styles) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            const blockTags = this._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'li', 'td', 'th', 'ul', 'ol', 'table']);
            
            // Get all block elements within the selection
            const blocks = [];
            
            // If selection is collapsed (just cursor), find the current block
            if (range.collapsed) {
                let currentBlock = range.commonAncestorContainer;
                if (currentBlock.nodeType === Node.TEXT_NODE) {
                    currentBlock = currentBlock.parentElement;
                }
                
                while (currentBlock && currentBlock !== editor && !blockTags.has(currentBlock.tagName?.toLowerCase())) {
                    currentBlock = currentBlock.parentElement;
                }
                
                if (currentBlock && currentBlock !== editor) {
                    blocks.push(currentBlock);
                }
            } else {
                // Selection spans content - find all blocks within selection
                const allBlocks = editor.querySelectorAll(Array.from(blockTags).join(','));
                
                for (let block of allBlocks) {
                    // Check if this block intersects with the selection
                    const blockRange = document.createRange();
                    blockRange.selectNodeContents(block);
                    
                    // Check if ranges intersect
                    if (range.compareBoundaryPoints(Range.START_TO_END, blockRange) > 0 &&
                        range.compareBoundaryPoints(Range.END_TO_START, blockRange) < 0) {
                        blocks.push(block);
                    }
                }
            }

            // Apply styles to all selected blocks
            if (styles && blocks.length > 0) {
                blocks.forEach(block => {
                    Object.keys(styles).forEach(property => {
                        block.style.setProperty(property, styles[property]);
                    });
                });
            }
        },

        clearFormatting: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // If selection is collapsed, nothing to clear
            if (range.collapsed) return;

            // Get the selected content
            const fragment = range.extractContents();

            // Remove all formatting by converting to plain text and back
            const tempDiv = document.createElement('div');
            tempDiv.appendChild(fragment);

            // Remove all formatting tags
            const formattingTags = ['strong', 'b', 'em', 'i', 'u', 's', 'strike', 'code', 'sub', 'sup', 'span', 'font'];
            formattingTags.forEach(tag => {
                const elements = tempDiv.querySelectorAll(tag);
                elements.forEach(element => {
                    // Replace with text content
                    const parent = element.parentNode;
                    while (element.firstChild) {
                        parent.insertBefore(element.firstChild, element);
                    }
                    parent.removeChild(element);
                });
            });

            // Also remove style attributes from any remaining elements
            const allElements = tempDiv.querySelectorAll('*');
            allElements.forEach(element => {
                element.removeAttribute('style');
                element.removeAttribute('class');
            });

            // Remove empty paragraph tags
            const emptyParagraphs = tempDiv.querySelectorAll('p');
            emptyParagraphs.forEach(p => {
                if (p.innerHTML === '' || p.innerHTML === '&nbsp;' || p.textContent.trim() === '') {
                    p.remove();
                }
            });

            // Insert the cleaned content back
            const insertedNodes = [];
            while (tempDiv.firstChild) {
                const node = tempDiv.firstChild;
                range.insertNode(node);
                insertedNodes.push(node);
                range.setStartAfter(node);
                range.setEndAfter(node);
            }

            // Restore selection
            if (insertedNodes.length > 0) {
                const newRange = document.createRange();
                newRange.setStartBefore(insertedNodes[0]);
                newRange.setEndAfter(insertedNodes[insertedNodes.length - 1]);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }

            // Clean up empty paragraphs at the start and end of editor
            const cleanupEmpty = (element) => {
                return element && 
                       element.tagName === 'P' && 
                       (element.innerHTML === '' || element.innerHTML === '&nbsp;' || element.textContent.trim() === '');
            };

            while (cleanupEmpty(editor.firstElementChild)) {
                editor.firstElementChild.remove();
            }
            
            while (cleanupEmpty(editor.lastElementChild)) {
                editor.lastElementChild.remove();
            }
        },

        cleanHtml: function(editorId) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // If selection is collapsed, nothing to clean
            if (range.collapsed) return;

            // Get the selected content
            const fragment = range.extractContents();

            // Create a temporary container
            const tempDiv = document.createElement('div');
            tempDiv.appendChild(fragment);

            // Extract text from block-level elements to preserve structure
            const blockElements = ['P', 'DIV', 'H1', 'H2', 'H3', 'H4', 'H5', 'H6', 'LI', 'BLOCKQUOTE', 'PRE', 'TD', 'TH'];
            const lines = [];

            // Helper to get only direct text content (excluding nested block elements)
            const getDirectTextContent = (element) => {
                let text = '';
                for (const node of element.childNodes) {
                    if (node.nodeType === Node.TEXT_NODE) {
                        text += node.textContent;
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        // Skip nested block elements (like nested UL/OL)
                        if (!['UL', 'OL'].includes(node.tagName)) {
                            // For inline elements, get their text
                            text += node.textContent;
                        }
                    }
                }
                return text.trim();
            };

            // Recursive function to extract text from block elements
            const extractTextLines = (element) => {
                for (const node of element.childNodes) {
                    if (node.nodeType === Node.TEXT_NODE) {
                        const text = node.textContent?.trim();
                        if (text) {
                            lines.push(text);
                        }
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        if (blockElements.includes(node.tagName)) {
                            // Block element - extract only its direct text (not nested blocks)
                            const text = getDirectTextContent(node);
                            if (text) {
                                lines.push(text);
                            }
                            // Only recurse if this block might contain other blocks (like LI with nested UL)
                            const hasNestedLists = node.querySelector('ul, ol');
                            if (hasNestedLists) {
                                extractTextLines(node);
                            }
                        } else if (['UL', 'OL'].includes(node.tagName)) {
                            // List container - recurse into it to find LI elements
                            extractTextLines(node);
                        } else {
                            // Inline element or other - recurse to find block children
                            extractTextLines(node);
                        }
                    }
                }
            };

            extractTextLines(tempDiv);

            // Create clean paragraph elements
            const cleanFragment = document.createDocumentFragment();
            
            if (lines.length === 0) {
                // If no text, insert a single paragraph
                const p = document.createElement('p');
                p.appendChild(document.createElement('br'));
                cleanFragment.appendChild(p);
            } else {
                lines.forEach(line => {
                    const p = document.createElement('p');
                    p.textContent = line;
                    cleanFragment.appendChild(p);
                });
            }

            // Collect nodes before insertion (as they'll be moved from fragment to DOM)
            const nodesToInsert = Array.from(cleanFragment.childNodes);

            // Insert the cleaned content back
            range.insertNode(cleanFragment);

            // Update selection to cover the inserted content
            if (nodesToInsert.length > 0) {
                const newRange = document.createRange();
                newRange.setStartBefore(nodesToInsert[0]);
                newRange.setEndAfter(nodesToInsert[nodesToInsert.length - 1]);
                selection.removeAllRanges();
                selection.addRange(newRange);
            }

            // Clean up any empty lists left in the editor after extraction
            const emptyLists = editor.querySelectorAll('ul:empty, ol:empty, ul:has(> li:empty:only-child), ol:has(> li:empty:only-child)');
            emptyLists.forEach(list => list.remove());
        },

        _notifyContentModified: function(editorId) {
            // Notify Blazor that content has been modified
            const callback = window.ZauberRTE._contentCallbacks?.[editorId];
            if (callback && callback.dotNetRef && callback.callbackMethod) {
                callback.dotNetRef.invokeMethodAsync(callback.callbackMethod);
            }
        },

        setupEnterKeyListener: function(editorId, dotNetRef, callbackMethod) {
            const editor = document.getElementById(editorId + '-content');
            if (editor) {
                // Store the callback reference
                if (!window.ZauberRTE._contentCallbacks) {
                    window.ZauberRTE._contentCallbacks = {};
                }
                window.ZauberRTE._contentCallbacks[editorId] = { dotNetRef, callbackMethod };

                // Prevent default link navigation in editor
                editor.addEventListener('click', (event) => {
                    if (event.target.tagName?.toLowerCase() === 'a') {
                        event.preventDefault(); // Always prevent default navigation
                        
                        // Ctrl+Click to open links in new window
                        if (event.ctrlKey || event.metaKey) {
                            const href = event.target.getAttribute('href');
                            if (href) {
                                window.open(href, event.target.getAttribute('target') || '_blank');
                            }
                        }
                    }
                });

                editor.addEventListener('keydown', (event) => {
                    if (event.key === 'Enter') {
                        if (event.shiftKey) {
                            // Shift+Enter - insert line break instead of new paragraph
                            event.preventDefault();
                            event.stopPropagation();
                            const selection = window.getSelection();
                            if (selection.rangeCount) {
                                const range = selection.getRangeAt(0);
                                range.deleteContents();
                                const br = document.createElement('br');
                                range.insertNode(br);
                                range.setStartAfter(br);
                                range.setEndAfter(br);
                                selection.removeAllRanges();
                                selection.addRange(range);
                            }
                            // Notify Blazor of content change
                            window.ZauberRTE.selection._notifyContentModified(editorId);
                        } else {
                            // Regular Enter - new paragraph/list item
                            event.preventDefault();
                            event.stopPropagation();
                            this.handleEnterKey(editorId);
                            // Notify Blazor of content change
                            window.ZauberRTE.selection._notifyContentModified(editorId);
                        }
                    } else if (event.key === 'Tab') {
                        // Tab/Shift+Tab in lists and tables
                        event.preventDefault();
                        if (this.handleTabKey(editorId, event.shiftKey)) {
                            // Notify Blazor of content change after Tab handling
                            window.ZauberRTE.selection._notifyContentModified(editorId);
                            return;
                        }
                    } else if (window.ZauberRTE.shortcuts.shouldPreventDefault(editorId, event)) {
                        // Prevent default browser behavior for registered shortcuts
                        // Let Blazor handle these via custom toolbar items
                        // NOTE: Only preventDefault, NOT stopPropagation - Blazor needs to receive the event
                        event.preventDefault();
                    } else if (event.key === 'Backspace' && !event.shiftKey) {
                        // Backspace at start of list item
                        if (this.handleBackspaceInList(editorId, event)) {
                            // Notify Blazor of content change
                            window.ZauberRTE.selection._notifyContentModified(editorId);
                            return;
                        }
                        // Handle deletion of empty paragraphs
                        this.handleDeletion(editorId, event);
                    } else if (event.key === 'Delete' && !event.shiftKey) {
                        // Handle deletion of empty paragraphs
                        this.handleDeletion(editorId, event);
                    }
                });

                // Handle input events for paragraph cleanup
                editor.addEventListener('input', (event) => {
                    this.handleParagraphInput(editorId, event.target);
                    // Clean up any spans that might have been created during input
                    this.cleanUnnecessarySpans(editor);
                });

                // Handle deletion keys - cleanup empty paragraphs immediately after
                editor.addEventListener('keyup', (event) => {
                    if (event.key === 'Backspace' || event.key === 'Delete') {
                        // Clean up empty paragraphs after deletion and notify
                        this.cleanupEmptyParagraphs(editorId);
                    }
                });

                // Handle focus events to clean up empty paragraphs
                editor.addEventListener('focusin', (event) => {
                    this.handleParagraphFocus(editorId, event.target);
                });
            }
        },

        cleanupEmptyParagraphs: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            const paragraphs = editor.querySelectorAll('p');
            const selection = window.getSelection();
            const currentRange = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
            let cursorParagraph = null;

            // Find which paragraph has the cursor
            if (currentRange) {
                let node = currentRange.startContainer;
                if (node.nodeType === Node.TEXT_NODE) {
                    node = node.parentElement;
                }
                while (node && node !== editor) {
                    if (node.tagName?.toLowerCase() === 'p') {
                        cursorParagraph = node;
                        break;
                    }
                    node = node.parentElement;
                }
            }

            // Remove empty paragraphs that don't have the cursor
            let removedAny = false;
            paragraphs.forEach(p => {
                if (p !== cursorParagraph && (p.innerHTML === '<br>' || p.innerHTML === '&nbsp;' || p.textContent.trim() === '')) {
                    p.remove();
                    removedAny = true;
                }
            });

            // Notify Blazor if we removed any paragraphs
            if (removedAny) {
                this._notifyContentModified(editorId);
            }
        },

        handleParagraphInput: function(editorId, target) {
            // Find the paragraph element - might be the target itself or a parent
            let paragraph = target;
            if (target.tagName?.toLowerCase() !== 'p') {
                // Walk up to find the paragraph
                let node = target;
                while (node && node.tagName?.toLowerCase() !== 'p') {
                    node = node.parentElement;
                }
                paragraph = node;
            }
            
            // Check if we found a paragraph
            if (paragraph && paragraph.tagName?.toLowerCase() === 'p') {
                // Remove &nbsp; if it appears at the start and there's actual content after it
                if (paragraph.innerHTML.startsWith('&nbsp;') && paragraph.textContent?.trim()) {
                    // Save cursor position
                    const selection = window.getSelection();
                    const range = selection.rangeCount > 0 ? selection.getRangeAt(0) : null;
                    const startOffset = range ? range.startOffset : 0;
                    const startContainer = range ? range.startContainer : null;
                    
                    // Remove the leading &nbsp; and keep the rest
                    const contentWithoutNbsp = paragraph.innerHTML.substring(6); // Remove '&nbsp;' (6 chars)
                    if (contentWithoutNbsp) {
                        paragraph.innerHTML = contentWithoutNbsp;
                        
                        // Restore cursor position (adjusted for the removed &nbsp;)
                        if (range && startContainer && paragraph.contains(startContainer)) {
                            try {
                                const newRange = document.createRange();
                                newRange.setStart(startContainer, Math.max(0, startOffset - 1));
                                newRange.collapse(true);
                                selection.removeAllRanges();
                                selection.addRange(newRange);
                            } catch (e) {
                                // If cursor restoration fails, just focus at the end
                                const newRange = document.createRange();
                                newRange.selectNodeContents(paragraph);
                                newRange.collapse(false);
                                selection.removeAllRanges();
                                selection.addRange(newRange);
                            }
                        }
                    } else {
                        // If only &nbsp; remains, clear it completely
                        paragraph.innerHTML = '';
                    }
                }

                // Clean up unnecessary spans with default styles
                this.cleanUnnecessarySpans(paragraph);
            }
        },

        cleanUnnecessarySpans: function(element) {
            // Find all spans in the element
            const spans = element.querySelectorAll('span');

            for (let i = 0; i < spans.length; i++) {
                const span = spans[i];
                // Check if span has only default style attributes
                const style = span.getAttribute('style');
                if (style) {
                    // Simple check for known default style patterns
                    if (style.indexOf('color: inherit') !== -1 &&
                        style.indexOf('font-size: inherit') !== -1 &&
                        style.indexOf('font-family: inherit') !== -1) {
                        // This span has the common default styles, unwrap it
                        const parent = span.parentNode;
                        if (parent) {
                            while (span.firstChild) {
                                parent.insertBefore(span.firstChild, span);
                            }
                            parent.removeChild(span);
                            i--; // Adjust index since we removed an element
                        }
                    }
                }
            }
        },

        handleDeletion: function(editorId, event) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return;

            const range = selection.getRangeAt(0);
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // Check if we're at the beginning of an empty paragraph with only &nbsp;
            if (range.collapsed && range.startOffset === 0) {
                let currentElement = range.startContainer;
                if (currentElement.nodeType === Node.TEXT_NODE) {
                    currentElement = currentElement.parentElement;
                }

                // Find the paragraph containing the cursor
                while (currentElement && currentElement !== editor) {
                    if (currentElement.tagName?.toLowerCase() === 'p') {
                        // Check if this paragraph contains only &nbsp;/br or is empty
                        if (currentElement.innerHTML === '&nbsp;' || currentElement.innerHTML === '<br>' ||
                            (currentElement.textContent?.trim() === '' && currentElement.children.length === 0)) {

                            // Check if there are other paragraphs
                            const allParagraphs = editor.querySelectorAll('p');
                            if (allParagraphs.length > 1) {
                                event.preventDefault();
                                event.stopPropagation();

                                // Find the previous paragraph to move cursor to
                                const prevParagraph = currentElement.previousElementSibling;
                                if (prevParagraph && prevParagraph.tagName?.toLowerCase() === 'p') {
                                    // Clean up any unnecessary spans in the previous paragraph
                                    this.cleanUnnecessarySpans(prevParagraph);

                                    // Move cursor to end of previous paragraph
                                    const textNodes = this.getTextNodes(prevParagraph);
                                    if (textNodes.length > 0) {
                                        const lastTextNode = textNodes[textNodes.length - 1];
                                        range.setStart(lastTextNode, lastTextNode.length);
                                        range.setEnd(lastTextNode, lastTextNode.length);
                                    } else {
                                        range.setStart(prevParagraph, prevParagraph.childNodes.length);
                                        range.setEnd(prevParagraph, prevParagraph.childNodes.length);
                                    }
                                    selection.removeAllRanges();
                                    selection.addRange(range);
                                }

                                // Remove the empty paragraph
                                currentElement.remove();
                                return;
                            }
                        }
                        break;
                    }
                    currentElement = currentElement.parentElement;
                }
            }

            // Check if we're at the end of an empty paragraph with only &nbsp;
            if (range.collapsed) {
                let currentElement = range.startContainer;
                if (currentElement.nodeType === Node.TEXT_NODE) {
                    currentElement = currentElement.parentElement;
                }

                // Find the paragraph containing the cursor
                while (currentElement && currentElement !== editor) {
                    if (currentElement.tagName?.toLowerCase() === 'p') {
                        const textNodes = this.getTextNodes(currentElement);
                        const isAtEnd = textNodes.length === 0 ||
                                       (textNodes.length === 1 && textNodes[0].length === range.startOffset);

                        if (isAtEnd && (currentElement.innerHTML === '&nbsp;' || currentElement.innerHTML === '<br>')) {
                            // Check if there are other paragraphs
                            const allParagraphs = editor.querySelectorAll('p');
                            if (allParagraphs.length > 1) {
                                event.preventDefault();
                                event.stopPropagation();

                                // Find the next paragraph to move cursor to
                                const nextParagraph = currentElement.nextElementSibling;
                                if (nextParagraph && nextParagraph.tagName?.toLowerCase() === 'p') {
                                    // Clean up any unnecessary spans in the next paragraph
                                    this.cleanUnnecessarySpans(nextParagraph);

                                    // Move cursor to beginning of next paragraph
                                    range.setStart(nextParagraph, 0);
                                    range.setEnd(nextParagraph, 0);
                                    selection.removeAllRanges();
                                    selection.addRange(range);
                                }

                                // Remove the empty paragraph
                                currentElement.remove();
                                return;
                            }
                        }
                        break;
                    }
                    currentElement = currentElement.parentElement;
                }
            }
        },

        getTextNodes: function(element) {
            const textNodes = [];
            const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
            let node;
            while (node = walker.nextNode()) {
                textNodes.push(node);
            }
            return textNodes;
        },

        handleParagraphFocus: function(editorId, target) {
            // Clean up any empty paragraphs with only &nbsp; when focusing
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // Only clean up if the target paragraph is not empty and we're focusing on it
            if (target && target.tagName?.toLowerCase() === 'p' && target.textContent?.trim()) {
                // Find all paragraphs that contain only &nbsp; and are not the currently focused one
                const paragraphs = editor.querySelectorAll('p');
                paragraphs.forEach(p => {
                    if (p !== target && (p.innerHTML === '&nbsp;' || p.innerHTML === '<br>') && p.textContent?.trim() === '') {
                        // This is an empty paragraph with only &nbsp; that's not focused - clean it up
                        // But we need to be careful not to remove the last paragraph
                        const allParagraphs = editor.querySelectorAll('p');
                        if (allParagraphs.length > 1) {
                            p.remove();
                        }
                    }
                });
            }
        },

        handleTabKey: function(editorId, isShiftTab) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return false;

            const range = selection.getRangeAt(0);
            let element = range.commonAncestorContainer;
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            const editor = document.getElementById(editorId + '-content');
            if (!editor) return false;

            // Check if we're in a list item
            let listItem = element;
            while (listItem && listItem !== editor) {
                if (listItem.tagName?.toLowerCase() === 'li') {
                    return this.handleTabInList(listItem, isShiftTab, editor);
                }
                listItem = listItem.parentElement;
            }

            // Check if we're in a table cell
            let tableCell = element;
            while (tableCell && tableCell !== editor) {
                if (tableCell.tagName?.toLowerCase() === 'td' || tableCell.tagName?.toLowerCase() === 'th') {
                    return this.handleTabInTable(tableCell, isShiftTab, selection);
                }
                tableCell = tableCell.parentElement;
            }

            return false;
        },

        handleTabInList: function(listItem, isShiftTab, editor) {
            const parentList = listItem.parentElement;
            if (!parentList) return false;

            if (isShiftTab) {
                // Outdent - move to parent level
                const grandParentLi = parentList.parentElement?.closest('li');
                if (grandParentLi) {
                    // We're in a nested list, move up one level
                    const grandParentList = grandParentLi.parentElement;
                    grandParentList.insertBefore(listItem, grandParentLi.nextSibling);
                    if (parentList.children.length === 0) {
                        parentList.remove();
                    }
                    return true;
                }
            } else {
                // Indent - nest under previous item
                const prevItem = listItem.previousElementSibling;
                if (prevItem && prevItem.tagName?.toLowerCase() === 'li') {
                    let nestedList = prevItem.querySelector(':scope > ul, :scope > ol');
                    if (!nestedList) {
                        nestedList = document.createElement(parentList.tagName);
                        prevItem.appendChild(nestedList);
                    }
                    nestedList.appendChild(listItem);
                    return true;
                }
            }

            return false;
        },

        handleTabInTable: function(cell, isShiftTab, selection) {
            const table = cell.closest('table');
            if (!table) return false;

            const allCells = Array.from(table.querySelectorAll('td, th'));
            const currentIndex = allCells.indexOf(cell);

            if (isShiftTab) {
                // Move to previous cell
                if (currentIndex > 0) {
                    const prevCell = allCells[currentIndex - 1];
                    const range = document.createRange();
                    range.selectNodeContents(prevCell);
                    range.collapse(true);
                    selection.removeAllRanges();
                    selection.addRange(range);
                    return true;
                }
            } else {
                // Move to next cell
                if (currentIndex < allCells.length - 1) {
                    const nextCell = allCells[currentIndex + 1];
                    const range = document.createRange();
                    range.selectNodeContents(nextCell);
                    range.collapse(true);
                    selection.removeAllRanges();
                    selection.addRange(range);
                    return true;
                }
            }

            return false;
        },

        handleBackspaceInList: function(editorId, event) {
            const selection = window.getSelection();
            if (!selection.rangeCount) return false;

            const range = selection.getRangeAt(0);
            if (!range.collapsed || range.startOffset !== 0) return false;

            let element = range.startContainer;
            if (element.nodeType === Node.TEXT_NODE) {
                element = element.parentElement;
            }

            const editor = document.getElementById(editorId + '-content');
            if (!editor) return false;

            // Find the list item
            let listItem = element;
            while (listItem && listItem !== editor) {
                if (listItem.tagName?.toLowerCase() === 'li') {
                    const parentList = listItem.parentElement;
                    
                    // Check if we're in a nested list (has a grandparent list item)
                    const grandParentLi = parentList.parentElement?.closest('li');
                    
                    if (grandParentLi) {
                        // We're in a nested list - un-indent (outdent) instead of converting to paragraph
                        event.preventDefault();
                        
                        // Move list item up one level (same logic as Shift+Tab)
                        const grandParentList = grandParentLi.parentElement;
                        grandParentList.insertBefore(listItem, grandParentLi.nextSibling);
                        
                        // Clean up empty nested list
                        if (parentList.children.length === 0) {
                            parentList.remove();
                        }
                        
                        // Preserve cursor position at start of list item
                        const newRange = document.createRange();
                        newRange.setStart(listItem, 0);
                        newRange.collapse(true);
                        selection.removeAllRanges();
                        selection.addRange(newRange);
                        
                        return true;
                    }
                    
                    // Not nested - check if this is the first item
                    if (listItem === parentList.firstElementChild) {
                        event.preventDefault();
                        
                        // Convert to paragraph
                        const p = document.createElement('p');
                        while (listItem.firstChild) {
                            p.appendChild(listItem.firstChild);
                        }
                        if (!p.textContent.trim()) {
                            p.innerHTML = '<br>';
                        }
                        
                        // Insert before list
                        parentList.parentNode.insertBefore(p, parentList);
                        parentList.removeChild(listItem);
                        
                        // Remove list if empty
                        if (parentList.children.length === 0) {
                            parentList.remove();
                        }
                        
                        // Set cursor
                        const newRange = document.createRange();
                        newRange.setStart(p, 0);
                        newRange.collapse(true);
                        selection.removeAllRanges();
                        selection.addRange(newRange);
                        
                        return true;
                    } else {
                        // Merge with previous item
                        const prevItem = listItem.previousElementSibling;
                        if (prevItem && prevItem.tagName?.toLowerCase() === 'li') {
                            event.preventDefault();
                            
                            // Move content to previous item
                            while (listItem.firstChild) {
                                prevItem.appendChild(listItem.firstChild);
                            }
                            listItem.remove();
                            
                            // Set cursor at merge point
                            const newRange = document.createRange();
                            newRange.selectNodeContents(prevItem);
                            newRange.collapse(false);
                            selection.removeAllRanges();
                            selection.addRange(newRange);
                            
                            return true;
                        }
                    }
                    
                    return false;
                }
                listItem = listItem.parentElement;
            }

            return false;
        }
    },

    // Clipboard API
    clipboard: {
        _pasteCallback: null,

        setPasteCallback: function(dotNetRef, methodName) {
            this._pasteCallback = dotNetRef;
            this._pasteMethodName = methodName;
        },

        handlePaste: async function(editorId, event) {
            // Prevent default paste behavior
            event.preventDefault();

            // Get clipboard data
            const clipboardData = event.clipboardData || window.clipboardData;
            const html = clipboardData.getData('text/html');
            const text = clipboardData.getData('text/plain');
            const content = html || text || '';

            // Call the .NET callback with the data
            if (this._pasteCallback && this._pasteMethodName) {
                await this._pasteCallback.invokeMethodAsync(this._pasteMethodName, content);
            }
        },

        cleanHtml: function(html, policy) {
            // ALWAYS remove HTML comments first (including StartFragment/EndFragment from Windows clipboard)
            html = html.replace(/<!--[\s\S]*?-->/g, '');
            
            // Remove XML processing instructions that sometimes come from Word
            html = html.replace(/<\?xml[^>]*>/gi, '');
            
            let cleaned = html;

            // Try to use DOMPurify if available for robust XSS protection
            if (typeof DOMPurify !== 'undefined') {
                // Configure DOMPurify to be very strict - only allow minimal attributes
                const config = {
                    ALLOWED_TAGS: policy.allowedTags || ['p', 'br', 'div', 'span', 'strong', 'em', 'b', 'i', 'u', 's', 'sub', 'sup', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'code', 'ul', 'ol', 'li', 'table', 'thead', 'tbody', 'tr', 'th', 'td', 'a', 'img', 'figure', 'figcaption', 'hr'],
                    ALLOWED_ATTR: ['href', 'src', 'alt', 'colspan', 'rowspan'], // Only truly essential attributes
                    KEEP_CONTENT: true,
                    ALLOW_DATA_ATTR: false
                };

                // Clean the HTML with DOMPurify
                cleaned = DOMPurify.sanitize(html, config);
            } else {
                console.warn('DOMPurify not loaded, using fallback cleaning');
            }

            // Apply Word cleaner if it's a Word document
            cleaned = this.cleanWordHtml(cleaned);

            // Additional aggressive cleaning - remove ALL remaining styles and attributes
            // This acts as both a post-DOMPurify cleanup and a fallback when DOMPurify isn't available
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = cleaned;
            
            // Build allowed attributes map from policy
            const allowedAttrMap = {};
            if (policy.allowedAttributes) {
                // Handle global attributes (apply to all tags)
                const globalAttrs = policy.allowedAttributes['*'] || [];
                
                // Build map for each tag in the policy
                Object.keys(policy.allowedAttributes).forEach(tag => {
                    const attrs = policy.allowedAttributes[tag] || [];
                    allowedAttrMap[tag.toLowerCase()] = new Set([...globalAttrs, ...attrs]);
                });
            }
            
            // Strip all unwanted attributes from all elements
            const allElements = tempDiv.querySelectorAll('*');
            allElements.forEach(element => {
                const tagName = element.tagName.toLowerCase();
                
                // Get allowed attributes for this tag from policy, or use minimal defaults
                let attributesToKeep = allowedAttrMap[tagName] || allowedAttrMap['*'] || new Set();
                
                // If no policy specified for this tag, fall back to minimal safe defaults
                if (attributesToKeep.size === 0) {
                    const defaults = [];
                    if (tagName === 'a') {
                        defaults.push('href');
                    } else if (tagName === 'img') {
                        defaults.push('src', 'alt');
                    } else if (tagName === 'td' || tagName === 'th') {
                        defaults.push('colspan', 'rowspan');
                    }
                    attributesToKeep = new Set(defaults);
                }
                
                // Remove all attributes except those in the keep list
                const attributesToRemove = [];
                for (let i = 0; i < element.attributes.length; i++) {
                    const attr = element.attributes[i];
                    // Remove any attribute not in the keep list
                    // This includes style, class, id, data-*, and all CSS variables (--*)
                    if (!attributesToKeep.has(attr.name)) {
                        attributesToRemove.push(attr.name);
                    }
                }
                attributesToRemove.forEach(attr => element.removeAttribute(attr));
            });

            // Remove empty tags (except br, img, hr)
            const emptyElements = tempDiv.querySelectorAll('*:not(br):not(img):not(hr)');
            emptyElements.forEach(element => {
                if (!element.textContent.trim() && element.children.length === 0) {
                    element.remove();
                }
            });

            return tempDiv.innerHTML;
        },

        buildAllowedAttributes: function(policy) {
            const allowed = {};

            // Global attributes
            const globalAttrs = policy.allowedAttributes['*'] || [];
            globalAttrs.forEach(attr => {
                allowed[attr] = true;
            });

            // Tag-specific attributes
            Object.keys(policy.allowedAttributes).forEach(tag => {
                if (tag === '*') return;
                const attrs = policy.allowedAttributes[tag] || [];
                attrs.forEach(attr => {
                    if (!allowed[attr]) allowed[attr] = [];
                    if (!allowed[attr].includes(tag)) {
                        allowed[attr].push(tag);
                    }
                });
            });

            return allowed;
        },

        cleanWordHtml: function(html) {
            // Remove Word-specific styles and classes
            html = html.replace(/mso-[^;]+;?/gi, '');
            html = html.replace(/class="?Mso[^"\s]*"?/gi, '');
            html = html.replace(/style="[^"]*mso-[^"]*"/gi, '');
            html = html.replace(/<!--\[if [^>]*\]>.*?<!\[endif\]-->/gi, '');

            // Convert Word lists to semantic lists
            html = this.convertWordLists(html);

            // Upgrade legacy tags
            html = html.replace(/<b\b([^>]*)>/gi, '<strong$1>');
            html = html.replace(/<\/b>/gi, '</strong>');
            html = html.replace(/<i\b([^>]*)>/gi, '<em$1>');
            html = html.replace(/<\/i>/gi, '</em>');

            // Remove font tags
            html = html.replace(/<font[^>]*>/gi, '');
            html = html.replace(/<\/font>/gi, '');

            return html;
        },

        convertWordLists: function(html) {
            // Convert Word's complex list structures to semantic HTML lists
            // Word uses various formats for lists, including:
            // - p elements with specific styles
            // - Custom list prefixes/suffixes
            // - Nested list structures

            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            this.convertWordListElements(tempDiv);
            this.normalizeListStructure(tempDiv);

            return tempDiv.innerHTML;
        },

        convertWordListElements: function(container) {
            // Find all paragraphs that might be list items
            const paragraphs = container.querySelectorAll('p');
            const listItems = [];
            let currentListType = null;
            let currentListLevel = 0;

            paragraphs.forEach(p => {
                const style = p.getAttribute('style') || '';
                const text = p.textContent.trim();

                // Check for Word list patterns
                const isBulletList = this.isWordBulletList(p, style, text);
                const isNumberedList = this.isWordNumberedList(p, style, text);

                if (isBulletList || isNumberedList) {
                    const listType = isBulletList ? 'ul' : 'ol';
                    const listLevel = this.getWordListLevel(style);

                    // If this is a new list or different type/level, start a new list
                    if (listType !== currentListType || listLevel !== currentListLevel) {
                        this.finalizeCurrentList(listItems, currentListType, currentListLevel, container, p);
                        currentListType = listType;
                        currentListLevel = listLevel;
                        listItems.length = 0; // Clear previous list
                    }

                    // Remove the list marker from the text
                    const cleanText = this.removeWordListMarker(text, isBulletList);
                    listItems.push({ element: p, text: cleanText, level: listLevel });
                } else {
                    // Not a list item - finalize any pending list
                    if (listItems.length > 0) {
                        this.finalizeCurrentList(listItems, currentListType, currentListLevel, container, p);
                        listItems.length = 0;
                        currentListType = null;
                        currentListLevel = 0;
                    }
                }
            });

            // Finalize any remaining list
            if (listItems.length > 0) {
                this.finalizeCurrentList(listItems, currentListType, currentListLevel, container, null);
            }
        },

        isWordBulletList: function(p, style, text) {
            // Check for Word bullet list indicators
            const bulletMarkers = ['•', '◦', '▪', '▫', '●', '○', '■', '□', '◆', '◇'];
            const firstChar = text.charAt(0);

            // Check for bullet characters
            if (bulletMarkers.includes(firstChar)) {
                return true;
            }

            // Check for Word-specific styles
            if (style.includes('mso-list') || style.includes('margin-left')) {
                // Additional checks for bullet patterns
                const bulletPatterns = [
                    /^\s*[•◦▪▫●○■□◆◇]\s+/,
                    /^\s*-\s+/,
                    /^\s*\*\s+/
                ];

                return bulletPatterns.some(pattern => pattern.test(text));
            }

            return false;
        },

        isWordNumberedList: function(p, style, text) {
            // Check for Word numbered list indicators
            const numberedPatterns = [
                /^\s*\d+\.?\s+/,      // 1. 2. 3.
                /^\s*\d+\)\s+/,       // 1) 2) 3)
                /^\s*\([a-z]\)\s+/i,  // (a) (b) (c)
                /^\s*\([A-Z]\)\s+/i,  // (A) (B) (C)
                /^\s*[a-z]\.?\s+/i,   // a. b. c.
                /^\s*[A-Z]\.?\s+/i,   // A. B. C.
                /^\s*[ivxlcdm]+\.?\s+/i, // i. ii. iii. etc.
                /^\s*\d+\.\d+\.?\s+/, // 1.1. 1.2. etc.
            ];

            // Check for numbered patterns
            if (numberedPatterns.some(pattern => pattern.test(text))) {
                return true;
            }

            // Check for Word-specific styles
            if (style.includes('mso-list') || style.includes('margin-left')) {
                return /^\s*\d+[\.)]\s+/.test(text) ||
                       /^\s*[a-zA-Z][\.)]\s+/.test(text) ||
                       /^\s*[ivxlcdm]+[\.)]\s+/i.test(text);
            }

            return false;
        },

        getWordListLevel: function(style) {
            // Estimate list level from indentation
            const marginMatch = style.match(/margin-left:\s*(\d+(?:\.\d+)?)(pt|px|em)/i);
            if (marginMatch) {
                const value = parseFloat(marginMatch[1]);
                const unit = marginMatch[2].toLowerCase();

                // Convert to approximate level (36pt = 0.5in typical indent)
                let pixels = value;
                if (unit === 'pt') pixels = value * 1.333; // Rough pt to px conversion
                if (unit === 'em') pixels = value * 16; // Rough em to px conversion

                return Math.max(0, Math.floor(pixels / 36));
            }

            // Check for text-indent as alternative
            const indentMatch = style.match(/text-indent:\s*(\d+(?:\.\d+)?)(pt|px|em)/i);
            if (indentMatch) {
                const value = parseFloat(indentMatch[1]);
                const unit = indentMatch[2].toLowerCase();

                let pixels = value;
                if (unit === 'pt') pixels = value * 1.333;
                if (unit === 'em') pixels = value * 16;

                return Math.max(0, Math.floor(pixels / 36));
            }

            return 0;
        },

        removeWordListMarker: function(text, isBulletList) {
            if (isBulletList) {
                // Remove bullet characters and extra whitespace
                return text.replace(/^[\s•◦▪▫●○■□◆◇\-*]+\s*/, '');
            } else {
                // Remove numbered markers
                return text.replace(/^[\s\d\w]+\.?\)?\s*/, '');
            }
        },

        finalizeCurrentList: function(listItems, listType, listLevel, container, insertBefore) {
            if (listItems.length === 0) return;

            // Create the list element
            const listElement = document.createElement(listType);

            // Add appropriate class for styling
            listElement.className = `word-converted-list level-${listLevel}`;

            listItems.forEach(item => {
                const li = document.createElement('li');

                // Move the content from the original p element to the li
                while (item.element.firstChild) {
                    li.appendChild(item.element.firstChild);
                }

                // Clean up any remaining Word-specific attributes
                li.removeAttribute('style');
                li.removeAttribute('class');

                listElement.appendChild(li);

                // Remove the original element
                if (item.element.parentNode) {
                    item.element.parentNode.removeChild(item.element);
                }
            });

            // Insert the list at the appropriate position
            if (insertBefore && insertBefore.parentNode) {
                insertBefore.parentNode.insertBefore(listElement, insertBefore);
            } else {
                container.appendChild(listElement);
            }
        },

        normalizeListStructure: function(container) {
            // Clean up any nested lists or malformed structures
            const lists = container.querySelectorAll('ul, ol');

            lists.forEach(list => {
                // Remove Word-specific classes and styles
                list.className = list.className.replace(/mso-list|word-converted-list|level-\d+/g, '').trim();
                list.removeAttribute('style');

                // Ensure all children are li elements
                const children = Array.from(list.children);
                children.forEach(child => {
                    if (child.tagName !== 'LI') {
                        const li = document.createElement('li');
                        li.innerHTML = child.outerHTML;
                        list.replaceChild(li, child);
                    }
                });
            });
        },

        insertCleanedHtml: function(editorId, html) {
            window.ZauberRTE.selection.insertHtml(editorId, html);
        },

        setupPasteListener: function(editorId) {
            const editor = document.getElementById(editorId);
            if (editor) {
                editor.addEventListener('paste', (event) => {
                    this.handlePaste(editorId, event);
                });
            }
        }
    },

    // Image API
    image: {
        _currentResizeImage: null,
        _resizeHandles: null,
        _isResizing: false,
        _originalDimensions: null,
        _maintainAspectRatio: true,
        _selectedImageInfo: new Map(), // editorId -> ImageInfo
        _editorObserver: null,

        showResizeHandles: function(editorId, imageId) {
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const imageElement = document.getElementById(imageId);
            if (!imageElement) return;
            this.hideResizeHandles();

            if (!imageElement) return;

            this._currentResizeImage = imageElement;
            this._originalDimensions = {
                width: imageElement.offsetWidth,
                height: imageElement.offsetHeight,
                naturalWidth: imageElement.naturalWidth,
                naturalHeight: imageElement.naturalHeight
            };

            // Create resize handles container
            const handlesContainer = document.createElement('div');
            handlesContainer.className = 'rte-image-resize-handles';
            handlesContainer.setAttribute('data-editor-id', editorId);

            // Create 4 corner resize handles only
            const positions = ['nw', 'ne', 'se', 'sw'];

            positions.forEach(pos => {
                const handle = document.createElement('div');
                handle.className = `rte-resize-handle rte-resize-handle-${pos}`;
                handle.setAttribute('data-position', pos);

                // Add mouse/touch event listeners
                handle.addEventListener('mousedown', (e) => this.startResize(e, pos, editorId));
                handle.addEventListener('touchstart', (e) => this.startResize(e, pos, editorId), { passive: false });

                handlesContainer.appendChild(handle);
            });

            // Position handles around the image
            this.updateHandlePositions(imageElement, handlesContainer);

            // Insert handles into body (outside content editable area)
            document.body.appendChild(handlesContainer);
            this._resizeHandles = handlesContainer;

            // Listen for image changes to update handle positions
            const imageObserver = new MutationObserver(() => {
                if (this._resizeHandles && document.contains(this._resizeHandles) && document.contains(imageElement)) {
                    this.updateHandlePositions(imageElement, this._resizeHandles);
                } else if (!document.contains(imageElement)) {
                    // Image was deleted, cleanup handles
                    this.hideResizeHandles();
                }
            });
            imageObserver.observe(imageElement, { attributes: true, attributeFilter: ['width', 'height', 'style'] });

            // Watch for image deletion from the editor
            const editorContent = document.getElementById(editorId + '-content');
            if (editorContent) {
                const editorObserver = new MutationObserver(() => {
                    if (!document.contains(imageElement)) {
                        this.hideResizeHandles();
                        this._selectedImageInfo.delete(editorId);
                        editorObserver.disconnect();
                    }
                });
                editorObserver.observe(editorContent, { childList: true, subtree: true });
                this._editorObserver = editorObserver;
            }
        },

        hideResizeHandles: function() {
            if (this._resizeHandles && document.contains(this._resizeHandles)) {
                this._resizeHandles.remove();
            }
            if (this._editorObserver) {
                this._editorObserver.disconnect();
                this._editorObserver = null;
            }
            this._resizeHandles = null;
            this._currentResizeImage = null;
            this._isResizing = false;
        },

        updateHandlePositions: function(imageElement, handlesContainer) {
            const rect = imageElement.getBoundingClientRect();
            const handles = handlesContainer.querySelectorAll('.rte-resize-handle');

            handles.forEach(handle => {
                const pos = handle.getAttribute('data-position');
                let left, top;

                switch (pos) {
                    case 'nw': left = rect.left - 6; top = rect.top - 6; break;
                    case 'n': left = rect.left + rect.width / 2 - 6; top = rect.top - 6; break;
                    case 'ne': left = rect.right - 6; top = rect.top - 6; break;
                    case 'e': left = rect.right - 6; top = rect.top + rect.height / 2 - 6; break;
                    case 'se': left = rect.right - 6; top = rect.bottom - 6; break;
                    case 's': left = rect.left + rect.width / 2 - 6; top = rect.bottom - 6; break;
                    case 'sw': left = rect.left - 6; top = rect.bottom - 6; break;
                    case 'w': left = rect.left - 6; top = rect.top + rect.height / 2 - 6; break;
                }

                handle.style.left = left + 'px';
                handle.style.top = top + 'px';
            });
        },

        startResize: function(event, position, editorId) {
            event.preventDefault();
            event.stopPropagation();

            if (!this._currentResizeImage) return;

            this._isResizing = true;
            this._resizePosition = position;

            const image = this._currentResizeImage;
            const startRect = image.getBoundingClientRect();
            const startX = event.clientX || event.touches[0].clientX;
            const startY = event.clientY || event.touches[0].clientY;

            this._resizeStart = {
                x: startX,
                y: startY,
                width: startRect.width,
                height: startRect.height,
                aspectRatio: startRect.width / startRect.height
            };

            // Add global event listeners for mouse/touch move and end
            const moveHandler = (e) => this.doResize(e, position);
            const endHandler = () => this.endResize();

            document.addEventListener('mousemove', moveHandler);
            document.addEventListener('touchmove', moveHandler, { passive: false });
            document.addEventListener('mouseup', endHandler);
            document.addEventListener('touchend', endHandler);

            // Prevent text selection during resize
            document.body.style.userSelect = 'none';
            document.body.style.webkitUserSelect = 'none';
        },

        doResize: function(event, position) {
            if (!this._isResizing || !this._currentResizeImage || !this._resizeStart) return;

            event.preventDefault();

            const image = this._currentResizeImage;
            const clientX = event.clientX || event.touches[0].clientX;
            const clientY = event.clientY || event.touches[0].clientY;

            const deltaX = clientX - this._resizeStart.x;
            const deltaY = clientY - this._resizeStart.y;

            let newWidth = this._resizeStart.width;
            let newHeight = this._resizeStart.height;

            // Calculate new dimensions based on handle position
            switch (position) {
                case 'e':
                    newWidth = Math.max(20, this._resizeStart.width + deltaX);
                    if (this._maintainAspectRatio) {
                        newHeight = newWidth / this._resizeStart.aspectRatio;
                    }
                    break;
                case 'w':
                    newWidth = Math.max(20, this._resizeStart.width - deltaX);
                    if (this._maintainAspectRatio) {
                        newHeight = newWidth / this._resizeStart.aspectRatio;
                    }
                    break;
                case 's':
                    newHeight = Math.max(20, this._resizeStart.height + deltaY);
                    if (this._maintainAspectRatio) {
                        newWidth = newHeight * this._resizeStart.aspectRatio;
                    }
                    break;
                case 'n':
                    newHeight = Math.max(20, this._resizeStart.height - deltaY);
                    if (this._maintainAspectRatio) {
                        newWidth = newHeight * this._resizeStart.aspectRatio;
                    }
                    break;
                case 'se':
                    newWidth = Math.max(20, this._resizeStart.width + deltaX);
                    newHeight = Math.max(20, this._resizeStart.height + deltaY);
                    if (this._maintainAspectRatio) {
                        // Use the dimension that changed more proportionally
                        const widthRatio = newWidth / this._resizeStart.width;
                        const heightRatio = newHeight / this._resizeStart.height;
                        const ratio = Math.max(widthRatio, heightRatio);
                        newWidth = this._resizeStart.width * ratio;
                        newHeight = this._resizeStart.height * ratio;
                    }
                    break;
                case 'sw':
                    newWidth = Math.max(20, this._resizeStart.width - deltaX);
                    newHeight = Math.max(20, this._resizeStart.height + deltaY);
                    if (this._maintainAspectRatio) {
                        const widthRatio = newWidth / this._resizeStart.width;
                        const heightRatio = newHeight / this._resizeStart.height;
                        const ratio = Math.max(widthRatio, heightRatio);
                        newWidth = this._resizeStart.width * ratio;
                        newHeight = this._resizeStart.height * ratio;
                    }
                    break;
                case 'ne':
                    newWidth = Math.max(20, this._resizeStart.width + deltaX);
                    newHeight = Math.max(20, this._resizeStart.height - deltaY);
                    if (this._maintainAspectRatio) {
                        const widthRatio = newWidth / this._resizeStart.width;
                        const heightRatio = newHeight / this._resizeStart.height;
                        const ratio = Math.max(widthRatio, heightRatio);
                        newWidth = this._resizeStart.width * ratio;
                        newHeight = this._resizeStart.height * ratio;
                    }
                    break;
                case 'nw':
                    newWidth = Math.max(20, this._resizeStart.width - deltaX);
                    newHeight = Math.max(20, this._resizeStart.height - deltaY);
                    if (this._maintainAspectRatio) {
                        const widthRatio = newWidth / this._resizeStart.width;
                        const heightRatio = newHeight / this._resizeStart.height;
                        const ratio = Math.max(widthRatio, heightRatio);
                        newWidth = this._resizeStart.width * ratio;
                        newHeight = this._resizeStart.height * ratio;
                    }
                    break;
            }

            // Apply new dimensions
            image.style.width = newWidth + 'px';
            image.style.height = newHeight + 'px';
            image.setAttribute('width', Math.round(newWidth));
            image.setAttribute('height', Math.round(newHeight));

            // Update handle positions
            if (this._resizeHandles) {
                this.updateHandlePositions(image, this._resizeHandles);
            }
        },

        endResize: function() {
            if (!this._isResizing) return;

            this._isResizing = false;

            // Remove global event listeners
            document.removeEventListener('mousemove', this.doResize);
            document.removeEventListener('touchmove', this.doResize);
            document.removeEventListener('mouseup', this.endResize);
            document.removeEventListener('touchend', this.endResize);

            // Restore text selection
            document.body.style.userSelect = '';
            document.body.style.webkitUserSelect = '';

            // Trigger change event for the editor
            if (this._currentResizeImage) {
                const editor = this._currentResizeImage.closest('[contenteditable]');
                if (editor) {
                    editor.dispatchEvent(new Event('input', { bubbles: true }));
                }
            }
        },

        resize: function(editorId, imageSelector, width, height, maintainAspectRatio) {
            // Legacy method - kept for compatibility
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const image = editor.querySelector(imageSelector);
            if (!image) return;

            image.style.width = width + 'px';
            image.style.height = height + 'px';
            image.setAttribute('width', width);
            image.setAttribute('height', height);
        },

        getDimensions: function(imageSrc) {
            return new Promise((resolve) => {
                const img = new Image();
                img.onload = () => resolve([img.width, img.height]);
                img.onerror = () => resolve([0, 0]);
                img.src = imageSrc;
            });
        },

        toggleAspectRatio: function() {
            this._maintainAspectRatio = !this._maintainAspectRatio;
            return this._maintainAspectRatio;
        },

        setupImageListeners: function(editorId, dotNetRef) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            // Use event delegation for dynamically added images
            editor.addEventListener('click', (event) => {
                const target = event.target;

                // Check if clicked element is an image
                if (target.tagName === 'IMG') {
                    event.preventDefault();
                    event.stopPropagation();

                    // Ensure image has an ID for reference
                    if (!target.id) {
                        target.id = 'img-' + Date.now() + '-' + Math.random().toString(36).substr(2, 9);
                    }

                    // Store the image info for later retrieval (survives toolbar button clicks)
                    const src = target.getAttribute('src') || '';
                    this._selectedImageInfo.set(editorId, {
                        src: src,
                        alt: target.getAttribute('alt'),
                        width: target.getAttribute('width') || target.style.width,
                        height: target.getAttribute('height') || target.style.height,
                        isDataUrl: src.startsWith('data:')
                    });

                    // Select the image in the DOM so it can be detected by getImageAtCursor
                    const selection = window.getSelection();
                    const range = document.createRange();
                    range.selectNode(target);
                    selection.removeAllRanges();
                    selection.addRange(range);

                    // Show resize handles directly
                    this.showResizeHandles(editorId, target.id);
                } else if (!event.target.closest('.rte-image-resize-handles')) {
                    // Hide resize handles when clicking elsewhere (but not on handles themselves)
                    this.hideResizeHandles();
                    // Clear stored image info
                    this._selectedImageInfo.delete(editorId);
                }
            });

            // Hide resize handles when clicking outside editor
            document.addEventListener('click', (event) => {
                const editorContent = document.getElementById(editorId + '-content');
                if (editorContent && !editorContent.contains(event.target) && !event.target.closest('.rte-image-resize-handles')) {
                    this.hideResizeHandles();
                    this._selectedImageInfo.delete(editorId);
                }
            });
        },

        clearSelectedImage: function(editorId) {
            this._selectedImageInfo.delete(editorId);
        }
    },

    // Mutation API
    mutation: {
        startObserving: function(editorId) {
        },

        stopObserving: function(editorId) {
        },

        getHtml: function(editorId) {
            const contentEditable = document.getElementById(editorId + '-content');
            if (!contentEditable) return '';

            // Ensure proper paragraph structure for clean HTML output
            const hasBlockElements = contentEditable.querySelector('p, div, h1, h2, h3, h4, h5, h6, blockquote, pre, ul, ol, li, table');
            if (!hasBlockElements && contentEditable.textContent?.trim()) {
                this.ensureParagraphStructure(editorId, contentEditable);
            }

            // Clone the content to avoid modifying the actual editor
            const clone = contentEditable.cloneNode(true);

            // Helper function to check if an element is an empty paragraph
            const isEmptyParagraph = (element) => {
                if (!element || element.nodeType !== Node.ELEMENT_NODE) return false;
                if (element.tagName !== 'P') return false;
                
                const html = element.innerHTML.trim();
                return html === '' || html === '<br>' || html === '<br/>' || html === '&nbsp;';
            };

            // Remove trailing empty paragraphs
            while (clone.lastElementChild && isEmptyParagraph(clone.lastElementChild)) {
                clone.removeChild(clone.lastElementChild);
            }

            // Remove leading empty paragraphs
            while (clone.firstElementChild && isEmptyParagraph(clone.firstElementChild)) {
                clone.removeChild(clone.firstElementChild);
            }

            return clone.innerHTML;
        },

        ensureParagraphStructure: function(editorId, contentEditable) {
            // If content is empty, add a default paragraph
            if (!contentEditable.innerHTML.trim()) {
                contentEditable.innerHTML = '<p><br></p>';
                return;
            }

            const children = Array.from(contentEditable.children);
            const blockTags = window.ZauberRTE.selection._blockTags.get(editorId) || new Set(['p', 'div', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'blockquote', 'pre', 'ul', 'ol', 'li', 'table']);

            // Check if we have any block-level elements
            const hasBlocks = children.some(child => blockTags.has(child.tagName?.toLowerCase()));

            if (!hasBlocks) {
                // Wrap all content in a paragraph
                const content = contentEditable.innerHTML;
                contentEditable.innerHTML = `<p>${content}</p>`;
            } else {
                // Ensure all top-level text nodes are wrapped in paragraphs
                const fragment = document.createDocumentFragment();
                let currentParagraph = null;

                Array.from(contentEditable.childNodes).forEach(node => {
                    if (node.nodeType === Node.TEXT_NODE && node.textContent?.trim()) {
                        // Text node that should be in a paragraph
                        if (!currentParagraph) {
                            currentParagraph = document.createElement('p');
                            fragment.appendChild(currentParagraph);
                        }
                        currentParagraph.appendChild(node);
                    } else if (node.nodeType === Node.ELEMENT_NODE) {
                        const tagName = node.tagName.toLowerCase();
                        if (blockTags.has(tagName)) {
                            // Block element - start new paragraph context
                            currentParagraph = null;
                            fragment.appendChild(node);
                        } else {
                            // Inline element - add to current paragraph or create one
                            if (!currentParagraph) {
                                currentParagraph = document.createElement('p');
                                fragment.appendChild(currentParagraph);
                            }
                            currentParagraph.appendChild(node);
                        }
                    } else if (node.nodeType === Node.TEXT_NODE && !node.textContent?.trim()) {
                        // Empty text node - skip (don't add whitespace-only nodes)
                    }
                });

                // Replace content with properly structured content
                if (fragment.children.length > 0) {
                    contentEditable.innerHTML = '';
                    contentEditable.appendChild(fragment);
                }
            }
        },

        setHtml: function(editorId, html) {
            const contentEditable = document.getElementById(editorId + '-content');
            if (contentEditable) {
                contentEditable.innerHTML = html;
                // Ensure proper structure after setting HTML
                this.ensureParagraphStructure(editorId, contentEditable);
            }
        },

        serializeHtml: function(editorId) {
            const contentEditable = document.getElementById(editorId + '-content');
            return contentEditable ? contentEditable.innerHTML : '';
        }
    },

    // Panel API
    panel: {
        open: function(editorId, panelId) {
        },

        close: function(editorId, panelId) {
        },

        trapFocus: function(panelId) {
        },

        releaseFocus: function(panelId) {
        }
    },

    // Resize API
    resize: {
        _isResizing: false,
        _startY: 0,
        _startHeight: 0,
        _editorId: null,

        setupResizeHandle: function(editorId) {
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const resizeHandle = editor.querySelector('.rte-editor-resize-handle');
            if (!resizeHandle) return;

            resizeHandle.addEventListener('mousedown', (e) => this.startResize(e, editorId));
            resizeHandle.addEventListener('touchstart', (e) => this.startResize(e, editorId), { passive: false });
        },

        startResize: function(event, editorId) {
            event.preventDefault();
            event.stopPropagation();

            this._isResizing = true;
            this._editorId = editorId;
            
            const editor = document.getElementById(editorId);
            if (!editor) return;

            const contentEditable = editor.querySelector('.rte-content, .rte-source-view');
            if (!contentEditable) return;

            this._startY = event.clientY || event.touches[0].clientY;
            this._startHeight = contentEditable.offsetHeight;

            // Add global event listeners
            const moveHandler = (e) => this.doResize(e);
            const endHandler = () => this.endResize();

            document.addEventListener('mousemove', moveHandler);
            document.addEventListener('touchmove', moveHandler, { passive: false });
            document.addEventListener('mouseup', endHandler);
            document.addEventListener('touchend', endHandler);

            // Store handlers for cleanup
            this._moveHandler = moveHandler;
            this._endHandler = endHandler;

            // Prevent text selection during resize
            document.body.style.userSelect = 'none';
            document.body.style.webkitUserSelect = 'none';
        },

        doResize: function(event) {
            if (!this._isResizing || !this._editorId) return;

            event.preventDefault();

            const editor = document.getElementById(this._editorId);
            if (!editor) return;

            const contentEditable = editor.querySelector('.rte-content, .rte-source-view');
            if (!contentEditable) return;

            const clientY = event.clientY || event.touches[0].clientY;
            const deltaY = clientY - this._startY;
            const newHeight = Math.max(200, this._startHeight + deltaY); // Minimum 200px

            contentEditable.style.minHeight = newHeight + 'px';
        },

        endResize: function() {
            if (!this._isResizing) return;

            this._isResizing = false;

            // Remove global event listeners
            if (this._moveHandler) {
                document.removeEventListener('mousemove', this._moveHandler);
                document.removeEventListener('touchmove', this._moveHandler);
            }
            if (this._endHandler) {
                document.removeEventListener('mouseup', this._endHandler);
                document.removeEventListener('touchend', this._endHandler);
            }

            // Restore text selection
            document.body.style.userSelect = '';
            document.body.style.webkitUserSelect = '';

            this._moveHandler = null;
            this._endHandler = null;
            this._editorId = null;
        }
    },

    // History API (Undo/Redo)
    history: {
        _history: new Map(), // editorId -> { states: [], currentIndex: -1, maxStates: 50 }

        _getHistory: function(editorId) {
            if (!this._history.has(editorId)) {
                this._history.set(editorId, {
                    states: [],
                    currentIndex: -1,
                    maxStates: 50
                });
            }
            return this._history.get(editorId);
        },

        recordState: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return;

            const history = this._getHistory(editorId);
            const currentHtml = editor.innerHTML;

            // Don't record if the content hasn't changed
            if (history.states.length > 0 &&
                history.states[history.currentIndex] === currentHtml) {
                return;
            }

            // Remove any states after current index (when user typed after undo)
            history.states = history.states.slice(0, history.currentIndex + 1);

            // Add new state
            history.states.push(currentHtml);
            history.currentIndex++;

            // Limit history size
            if (history.states.length > history.maxStates) {
                history.states.shift();
                history.currentIndex--;
            }
        },

        undo: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return false;

            const history = this._getHistory(editorId);

            if (!this.canUndo(editorId)) {
                return false;
            }

            // Move to previous state
            history.currentIndex--;
            const previousState = history.states[history.currentIndex];

            // Restore the state
            editor.innerHTML = previousState;

            // Trigger input event to notify components
            editor.dispatchEvent(new Event('input', { bubbles: true }));

            return true;
        },

        redo: function(editorId) {
            const editor = document.getElementById(editorId + '-content');
            if (!editor) return false;

            const history = this._getHistory(editorId);

            if (!this.canRedo(editorId)) {
                return false;
            }

            // Move to next state
            history.currentIndex++;
            const nextState = history.states[history.currentIndex];

            // Restore the state
            editor.innerHTML = nextState;

            // Trigger input event to notify components
            editor.dispatchEvent(new Event('input', { bubbles: true }));

            return true;
        },

        canUndo: function(editorId) {
            const history = this._getHistory(editorId);
            return history.currentIndex > 0;
        },

        canRedo: function(editorId) {
            const history = this._getHistory(editorId);
            return history.currentIndex < history.states.length - 1;
        },

        clearHistory: function(editorId) {
            this._history.delete(editorId);
        }
    }
};
